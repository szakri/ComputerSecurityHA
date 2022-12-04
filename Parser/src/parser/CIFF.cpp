#include "parser/CIFF.hpp"

#include <iostream>
#include <string>

CIFF::CIFF(std::istream& ciffContent) {
		parseHeader(ciffContent);
		parseImage(ciffContent);

		valid = true;
}

void CIFF::parseHeader(std::istream& ciffContent) {
	std::string fileMagic(MAGIC_LENGTH, '\0');
	ciffContent.read(fileMagic.data(), MAGIC_LENGTH);

	if(ciffContent.gcount() != MAGIC_LENGTH || !ciffContent.good() || fileMagic != MAGIC) {
		throw std::invalid_argument("The given content is not a valid CIFF file. Magic header mismatch.");
	}

	int64_t headerLength;
	ciffContent.read(reinterpret_cast<char*>(&headerLength), sizeof headerLength);
	if(!ciffContent.good() || headerLength < MINIMUM_HEADER_LENGTH) {
		throw std::invalid_argument("The given content is not a valid CIFF file. Invalid header size.");
	}

	int64_t contentLength;
	ciffContent.read(reinterpret_cast<char*>(&contentLength), sizeof contentLength);
	if(!ciffContent.good() || contentLength < 0) {
		throw std::invalid_argument("The given content is not a valid CIFF file. Invalid content length.");
	}

	parseDimensions(ciffContent);
	if(width * height * 3 != contentLength) {
		throw std::invalid_argument(
				"The given content is not a valid CIFF file. Content length and dimension mismatch.");
	}

	const std::size_t maximumCaptionLength = headerLength - CAPTION_START - 1; // -1 for the required 1 `tag` separator
	parseCaption(ciffContent, maximumCaptionLength);

	const std::size_t neededTagsLength =
			headerLength - caption.size() - CAPTION_START - 1; // -1 for caption closing character
	parseTags(ciffContent, neededTagsLength);
}

void CIFF::parseDimensions(std::istream& ciffContent) {
	ciffContent.read(reinterpret_cast<char*>(&width), sizeof width);
	if(!ciffContent.good() || width < 0) {
		throw std::invalid_argument("The given content is not a valid CIFF file. Negative image width.");
	}

	ciffContent.read(reinterpret_cast<char*>(&height), sizeof height);
	if(!ciffContent.good() || height < 0) {
		throw std::invalid_argument("The given content is not a valid CIFF file. Negative image height.");
	}
}

void CIFF::parseCaption(std::istream& ciffContent, const std::size_t& maximumCaptionLength) {
	std::getline(ciffContent, caption, CAPTION_END_CHARACTER);
	if(caption.length() > maximumCaptionLength || !ciffContent.good()) {
		throw std::invalid_argument("The given content is not a valid CIFF file. Caption length exceeds limits.");
	}
}

void CIFF::parseTags(std::istream& ciffContent, const std::size_t& neededTagsLength) {
	std::size_t readTagsLength = 0;
	do {
		std::string tag;
		std::getline(ciffContent, tag, TAG_SEPARATOR_CHARACTER);
		if(!ciffContent.good()) {
			throw std::invalid_argument("The given content is not a valid CIFF file. Could not read tags.");
		}

		if(tag.find('\n') != std::string::npos) {
			throw std::invalid_argument(
					"The given content is not a valid CIFF file. " + std::to_string(tags.size() + 1) +
					"th tag contained an invalid \\n character.");
		}

		unsigned long tagLength = tag.length();
		readTagsLength += tagLength + 1;

		if(tagLength > 0) {
			tags.emplace_back(tag);
		}
	} while(readTagsLength < neededTagsLength);
}

void CIFF::parseImage(std::istream& ciffContent) {
	int64_t pixelCount = width * height;
	try {
		image.resize(pixelCount);
	} catch (const std::bad_alloc &) {
		throw std::invalid_argument("Could not parse image info, as it exceeds the memory limit.");
	}

    for (unsigned i = 0; i < pixelCount; ++i) {
        ciffContent.read(reinterpret_cast<char*>(&image[i]), 3);
    }

	if(ciffContent.fail()) {
		throw std::invalid_argument("The given content is not a valid CIFF file. Could not read image.");
	}
}
