#ifndef BACKEND_CIFF_HPP
#define BACKEND_CIFF_HPP

#include <istream>
#include <vector>
#include "Color.hpp"

class CIFF
{
	public:
		explicit CIFF(std::istream& ciffContent);

		[[nodiscard]]
		bool isValid() const { return valid; };

		[[nodiscard]]
		int64_t getWidth() const { return width; };

		[[nodiscard]]
		int64_t getHeight() const { return height; };

		[[nodiscard]]
		std::string_view getCaption() const { return caption; };

		[[nodiscard]]
		std::vector<std::string> getTags() const { return tags; };

		[[nodiscard]]
		const std::vector<Color>& getImage() const { return image; };

	private:
		bool valid = false;
		int64_t width = 0;
		int64_t height = 0;
		std::string caption;
		std::vector<std::string> tags;
		std::vector<Color> image;

		static constexpr std::string_view MAGIC{"CIFF"};
		static constexpr std::size_t MAGIC_LENGTH = MAGIC.length();
		static constexpr std::size_t CAPTION_START = MAGIC_LENGTH +
		                                             sizeof(int64_t) * 4; // header size + content size + width + height
		static constexpr char CAPTION_END_CHARACTER = '\n';
		static constexpr char TAG_SEPARATOR_CHARACTER = '\0';
		static constexpr std::size_t MINIMUM_HEADER_LENGTH = CAPTION_START + sizeof(int64_t) * 2; // Caption + tags

		void parseHeader(std::istream& ciffContent);
		void parseDimensions(std::istream& ciffContent);
		void parseCaption(std::istream& ciffContent, const size_t& maxCaptionLength);
		void parseTags(std::istream& ciffContent, const size_t& neededTagsLength);
		void parseImage(std::istream& ciffContent);
};

#endif //BACKEND_CIFF_HPP
