export interface LoginDto {
    email: string;
    password: string;
}

export interface RegisterDto {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    confirmPassword: string;
    panNumber: string;
}

export interface TokenResponseDto {
    accessToken: string;
    refreshToken: string;
    accessTokenExpiresAt: string;
    tokenType: string;
}

export interface RegisterResponseDto {
    userId: string;
    email: string;
    firstName: string;
    lastName: string;
    panNumber: string;
    status: string;
    message: string;
}
