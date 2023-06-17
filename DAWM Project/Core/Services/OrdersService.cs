﻿using RestaurantAPI.Domain;
using RestaurantAPI.Domain.Dtos.OrderDtos;
using RestaurantAPI.Domain.Dtos.RecipeDtos;
using RestaurantAPI.Domain.Mapping;
using RestaurantAPI.Domain.Models.Orders;
using RestaurantAPI.Domain.ServicesAbstractions;
using RestaurantAPI.Exceptions;

namespace Core.Services
{
    internal class OrdersService : IOrdersService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IDataLogger logger;

        public OrdersService(IUnitOfWork unitOfWork, IDataLogger logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<bool> Create(CreateOrUpdateOrder order)
        {
            if (order == null)
            {
                logger.LogError($"Null argument from controller: {nameof(order)}");
                throw new ArgumentNullException(nameof(order));
            }

            Order orderData = OrderMapping.MapToOrder(order);

            if (orderData == null)
                return false;

            await _unitOfWork.OrdersRepository.AddAsync(orderData);

            bool result = await _unitOfWork.SaveChangesAsync();
            logger.LogInfo($"Order with id {orderData.Id} added");

            return result;
        }

        public async Task<bool> AddOrderItem(int orderId, int menuId)
        {
            Order order = await _unitOfWork.OrdersRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                logger.LogWarn($"Order with id {orderId} not found");
                return false;
            }

            if (await _unitOfWork.MenusRepository.GetByIdAsync(menuId) == null)
            {
                logger.LogWarn($"Menu with id {menuId} not found");
                return false;
            }
            var orderItm = order.OrderItems.FirstOrDefault(x => x.MenuId == menuId);
            if (orderItm == null)
                order.OrderItems.Add(new OrderItems { MenuId = menuId, Quantity = 1 });
            else
            {
                orderItm.Quantity++;
            }

            bool response = await _unitOfWork.SaveChangesAsync();
            logger.LogInfo($"Order with id {orderId} updated. Added Menu with id {menuId}");

            return response;
        }

        public async Task<bool> Delete(int orderId)
        {
            try
            {
                await _unitOfWork.OrdersRepository.DeleteAsync(orderId);
            }
            catch (EntityNotFoundException exception)
            {
                logger.LogError(exception.Message, exception);

                return false;
            }

            bool response = await _unitOfWork.SaveChangesAsync();
            logger.LogInfo($"Order with id {orderId} deleted");

            return response;
        }

        public async Task<bool> DeleteOrderItem(int orderId, int menuId)
        {
            Order order = await _unitOfWork.OrdersRepository.GetByIdAsync(orderId);
            if (order == null)
                if (order == null)
                {
                    logger.LogWarn($"Order with id {orderId} not found");
                    return false;
                }

            var record = order.OrderItems.Where(item => item.MenuId == menuId).FirstOrDefault();
            if (record == null)
            {
                logger.LogWarn($"Order with Id:{orderId} doesn't contains Menu with id {menuId}");
                return false;
            }

            order.OrderItems.Remove(record);

            bool response = await _unitOfWork.SaveChangesAsync();
            logger.LogInfo($"Order with id {orderId} updated. Removed Menu with id {menuId}");
            return response;
        }

        public async Task<IEnumerable<OrderInfo>> GetAll()
        {
            var ordersFromDb = await _unitOfWork.OrdersRepository.GetAllAsync();

            return ordersFromDb.Select(order => OrderMapping.MapToOrderInfos(order)).ToList();
        }

        public async Task<OrderInfo> GetById(int orderId)
        {
            var orderFromDb = await _unitOfWork.OrdersRepository.GetByIdAsync(orderId);

            return OrderMapping.MapToOrderInfos(orderFromDb);
        }

        public async Task<bool> Update(int orderId, CreateOrUpdateOrder order)
        {
            if (order == null)
            {
                logger.LogError($"Null argument from controller: {nameof(order)}");

                throw new ArgumentNullException(nameof(order));
            }

            Order orderData = OrderMapping.MapToOrder(order);

            try
            {
                await _unitOfWork.OrdersRepository.UpdateAsync(orderId, orderData);
            }
            catch (EntityNotFoundException exception)
            {
                logger.LogError(exception.Message, exception);

                return false;
            }
            catch (ArgumentNullException exception)
            {
                logger.LogError(exception.Message, exception);
                return false;
            }

            bool response = await _unitOfWork.SaveChangesAsync();
            logger.LogInfo($"Order with id {orderId} updated");

            return response;
        }
    }
}
