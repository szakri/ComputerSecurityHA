#ifndef BACKEND_COLOR_HPP
#define BACKEND_COLOR_HPP

struct Color
{
    uint8_t R;
    uint8_t G;
    uint8_t B;
    uint8_t A;

    Color(uint8_t R = 0, uint8_t G = 0, uint8_t B = 0, uint8_t A = 0) : R(R), G(G), B(B), A(A) {}
};

#endif //BACKEND_COLOR_HPP