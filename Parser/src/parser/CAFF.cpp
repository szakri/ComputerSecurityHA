#include "parser/CAFF.hpp"
#include <iostream>
#include <gif.h>
#include <string>

#if defined(_WIN32) && !defined(_XOPEN_SOURCE)
#define timezone _timezone
#endif

CAFF::CAFF(std::istream& caffContent) {
	parseBlocks(caffContent);
	valid = true;
}

void CAFF::writePreview(const char *filePath) {

    GifWriter g;
    GifBegin(&g, filePath, width, height, /* will specify delay for each image */ 0);

    for (auto& frame : frames) {
        unsigned delay = (unsigned)(frame.first.count() / 10.0f); /* milliseconds to hundredths of a second */
        GifWriteFrame(&g, reinterpret_cast<const uint8_t*>(frame.second.getImage().data()), frame.second.getWidth(), frame.second.getHeight(), delay);
    }

    GifEnd(&g);
}

void CAFF::parseBlocks(std::istream& caffContent) {
	parseHeader(caffContent);

	do {
		auto blockId = BlockID(caffContent.peek());
		if(caffContent.eof()) {
			break;
		}

		switch(blockId) {
			case BlockID::HEADER:
				parseHeader(caffContent);
				break;
			case BlockID::CREDITS:
				parseCredits(caffContent);
				break;
			case BlockID::FRAME:
				parseFrame(caffContent);
				break;
			default:
				throw std::invalid_argument("The given content is not a valid CAFF file. Invalid block id.");
		}
	} while(!caffContent.eof());

	unsigned long actualFrameCount = frames.size();
	if(actualFrameCount != expectedFrameCount) {
		throw std::invalid_argument("The given content is not a valid CAFF file. Missing frames. Expected: " +
		                            std::to_string(expectedFrameCount) + " Got: " + std::to_string(
				actualFrameCount));
	}

    width = frames[0].second.getWidth();
    height = frames[0].second.getHeight();

    /* validate frame sizes */
    for (auto& frame : frames) {
        if (frame.second.getWidth() != width || frame.second.getHeight() != height) {
            throw std::invalid_argument("Frame sizes are not uniform in the image.");
        }
    }
}

void CAFF::parseHeader(std::istream& caffContent) {
	uint64_t blockSize = CAFF::extractBlockInformation(caffContent, BlockID::HEADER);

	if(blockSize != REQUIRED_HEADER_LENGTH) {
		throw std::invalid_argument("The given content is not a valid CAFF file. Header size must match block size.");
	}

	std::string fileMagic(MAGIC_LENGTH, '\0');
	caffContent.read(fileMagic.data(), MAGIC_LENGTH);

	if(!caffContent.good() || fileMagic != MAGIC) {
		throw std::invalid_argument("The given content is not a valid CAFF file. Magic header mismatch.");
	}

	int64_t headerLength;
	caffContent.read(reinterpret_cast<char*>(&headerLength), sizeof headerLength);
	if(!caffContent.good() || headerLength != REQUIRED_HEADER_LENGTH) {
		throw std::invalid_argument("The given content is not a valid CAFF file. Invalid header size.");
	}

	int64_t previousFrameCount = expectedFrameCount;
	caffContent.read(reinterpret_cast<char*>(&expectedFrameCount), sizeof expectedFrameCount);
	if(!caffContent.good() || expectedFrameCount <= 0) {
		throw std::invalid_argument("The given content is not a valid CAFF file. Invalid frame count.");
	}

	if(previousFrameCount != 0 && previousFrameCount != expectedFrameCount) {
		throw std::invalid_argument(
				"The given content is not a valid CAFF file. Multiple header found with different frame counts.");
	}
}

void CAFF::parseCredits(std::istream& caffContent) {
	uint64_t blockSize = CAFF::extractBlockInformation(caffContent, BlockID::CREDITS);

	int16_t yearValue;
	caffContent.read(reinterpret_cast<char*>(&yearValue), sizeof yearValue);
	int8_t monthValue;
	caffContent.read(reinterpret_cast<char*>(&monthValue), sizeof monthValue);
	int8_t dayValue;
	caffContent.read(reinterpret_cast<char*>(&dayValue), sizeof dayValue);
	int8_t hourValue;
	caffContent.read(reinterpret_cast<char*>(&hourValue), sizeof hourValue);
	int8_t minuteValue;
	caffContent.read(reinterpret_cast<char*>(&minuteValue), sizeof minuteValue);

    std::tm tm = {
            0, /* seconds */
            minuteValue,
            hourValue,
            (dayValue),
            (monthValue) - 1,
            (yearValue) - 1900,
            0 /* no daytime saving */
    };
    createdAt = std::mktime(&tm) - timezone; // this is necessary to avoid timezone dependent results

	int64_t createdByLength;
	caffContent.read(reinterpret_cast<char*>(&createdByLength), sizeof createdByLength);

	if(!caffContent.good() || createdByLength < 0) {
		throw std::invalid_argument(
				"The given content is not a valid CAFF file. The creator length can not be negative.");
	}

	if(createdByLength + sizeof createdByLength != blockSize - CREATED_AT_LENGTH) {
		throw std::invalid_argument(
				"The given content is not a valid CAFF file. The creator information exceeds it's block size.");
	}

	try {
		createdBy.resize(createdByLength);
	} catch (const std::bad_alloc &) {
		throw std::invalid_argument("Could not parse creator info, as it exceeds the memory limit.");
	}
    if (createdByLength > 0) {
        caffContent.read(createdBy.data(), createdByLength);
    } else {
        createdBy = "<unknown>";
    }

}

void CAFF::parseFrame(std::istream& caffContent) {

	uint64_t blockSize = CAFF::extractBlockInformation(caffContent, BlockID::FRAME);

	const auto& blockStartPos = caffContent.tellg();
	int64_t duration;
	caffContent.read(reinterpret_cast<char*>(&duration), sizeof duration);
	if(!caffContent.good() || duration < 0) {
		throw std::invalid_argument("The given content is not a valid CAFF file. The duration can not be negative.");
	}

	CIFF imageFrame(caffContent);
	if(!imageFrame.isValid()) {
		throw std::invalid_argument(
				"The given content is not a valid CAFF file. The " + std::to_string(frames.size() + 1) +
				"th frame is invalid.");
	}

	long actualBlockSize = caffContent.tellg() - blockStartPos;
	if(blockSize != actualBlockSize) {
		throw std::invalid_argument(
				"The given content is not a valid CAFF file. The embedded CIFF file not matched it's container block size.");
	}

	frames.emplace_back(duration, imageFrame);
}

uint64_t CAFF::extractBlockInformation(std::istream& caffContent, const CAFF::BlockID& blockId) {
	auto extractedBlockId = BlockID(caffContent.get());
	if(extractedBlockId != blockId) {
		throw std::invalid_argument("The given content is not a valid CAFF file. Expected block id differs from actual."
		                            " Maybe first block is not header?");
	}

	int64_t blockSize;
	caffContent.read(reinterpret_cast<char*>(&blockSize), sizeof blockSize);
	if(blockSize <= 0) {
		throw std::invalid_argument("The given content is not a valid CAFF file. Block size must be positive.");
	}

	return blockSize;
}

[[nodiscard]]
std::vector<std::string_view> CAFF::getCaptions() const {
    std::vector<std::string_view> caption;

    for (auto& frame : frames) {
        caption.push_back(frame.second.getCaption());
    }

    return caption;
}

[[nodiscard]]
std::vector<std::vector<std::string>> CAFF::getTags() const {
    std::vector<std::vector<std::string>> tags;

    for (auto& frame : frames) {
        tags.push_back(frame.second.getTags());
    }

    return tags;
}
