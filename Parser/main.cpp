#include <parser/CAFF.hpp>

#include <fstream>
#include <iostream>

int main(int argc, char** argv) {
	if(argc >= 2) {
        try {
            std::ifstream caffFile(argv[1], std::ios::binary);
            CAFF caff(caffFile);

            std::cout << "Parsing CAFF video file..." << std::endl;

            auto creator = caff.getCreator();
            std::cout << "Author: " << creator << std::endl;

            const auto &createdAt = caff.getCreatedAt();
            std::cout << "Date of creation: " << asctime(gmtime(&createdAt));

            int64_t width = caff.getWidth();
            int64_t height = caff.getHeight();
            std::cout << "Resolution: " << width << "x" << height << std::endl;

            std::cout << "Caption: " << std::endl;
            unsigned frameIdx = 0;
            for (auto caption: caff.getCaptions()) {
                std::cout << "  Frame " << frameIdx << ":" << caption << std::endl;
                frameIdx++;
            }

            std::cout << "Tags: " << std::endl;
            frameIdx = 0;
            for (auto tags : caff.getTags()) { /* iterate through frame level caption collections */
                std::cout << "  Frame " << frameIdx << ":" <<  std::endl;

                unsigned tagIdx = 0;
                for (auto& tag : tags) { /* iterate through captions of the current frame */
                    std::cout << "    Tag " << tagIdx << ":" << tag << std::endl;
                    tagIdx++;
                }
                frameIdx++;
            }

            std::cout << "Valid: " << std::boolalpha << caff.isValid() << std::endl;

            if (argc == 3) {
                std::cout << "Creating gif segment..." << std::endl;

                char *outputPath = argv[2];
                caff.writePreview(outputPath); /* preview in gif format */
                std::cout << "Output written to file '" << outputPath << "'." << std::endl;
            }

            return 0;
        } catch (std::exception& e) {
            std::cerr << "An exception has occurred: " << e.what() << std::endl;
        }
    }

    std::cerr << "Usage: ./parser_demo.exe input.caff [output.gif]" << std::endl;
	return 1;
}
