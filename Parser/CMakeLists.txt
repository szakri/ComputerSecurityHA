cmake_minimum_required(VERSION 3.0)
project(parser)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fstack-protector-all")


include_directories("include")
include_directories("lib/gif-h")
add_executable(parser_demo main.cpp src/parser/CIFF.cpp include/parser/CIFF.hpp include/parser/Color.hpp src/parser/CAFF.cpp include/parser/CAFF.hpp)
add_library(parser SHARED src/parser/CIFF.cpp include/parser/CIFF.hpp include/parser/Color.hpp src/parser/CAFF.cpp include/parser/CAFF.hpp)
