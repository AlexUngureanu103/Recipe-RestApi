﻿using RestaurantAPI.Domain.Dtos.UserDtos;

namespace RestaurantAPI.Domain.ServicesAbstractions
{
    public interface IUsersService
    {
        Task<bool> Register(CreateOrUpdateUser registerData);

        Task<string> ValidateCredentials(LoginDto payload);

        Task<bool> DeleteAccount(int id);
    }
}
