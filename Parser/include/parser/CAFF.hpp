#ifndef BACKEND_CAFF_HPP
#define BACKEND_CAFF_HPP

#include "CIFF.hpp"

#include <cstdint>
#include <string_view>
#include <vector>
#include <chrono>

using Frame = std::pair<std::chrono::duration<int64_t, std::milli>, CIFF>;

class CAFF
{
public:
    explicit CAFF(std::istream& caffContent);

    [[nodiscard]]
    bool isValid() const { return valid; };

    void writePreview(const char *filePath);

    [[nodiscard]]
    const std::string & getCreator() const { return createdBy; };

    [[nodiscard]]
    const auto& getCreatedAt() const { return createdAt; };

    [[nodiscard]]
    int64_t getWidth() const { return width; };

    [[nodiscard]]
    int64_t getHeight() const { return height; };

    [[nodiscard]]
    std::vector<std::string_view> getCaptions() const;

    [[nodiscard]]
    std::vector<std::vector<std::string>> getTags() const;

private:
    bool valid = false;
    int64_t expectedFrameCount = 0;
    std::string createdBy;
    time_t createdAt;
    int64_t width;
    int64_t height;
    std::vector<Frame> frames;

    static constexpr std::string_view MAGIC{"CAFF"};
    static constexpr std::size_t MAGIC_LENGTH = MAGIC.length();
    static constexpr std::size_t REQUIRED_HEADER_LENGTH = MAGIC.length() + sizeof(int64_t) * 2;
    static constexpr std::size_t CREATED_AT_LENGTH = sizeof(int16_t) + sizeof(int8_t) * 4;

    enum class BlockID : int
    {
        HEADER = 0x1,
        CREDITS = 0x2,
        FRAME = 0x3
    };

    void parseBlocks(std::istream& caffContent);
    void parseHeader(std::istream& caffContent);
    void parseCredits(std::istream& caffContent);
    void parseFrame(std::istream& caffContent);

    static uint64_t extractBlockInformation(std::istream& caffContent, const BlockID &blockId);
};


#endif //BACKEND_CAFF_HPP
