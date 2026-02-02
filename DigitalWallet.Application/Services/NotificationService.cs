using AutoMapper;
using DigitalWallet.Application.Common;
using DigitalWallet.Application.DTOs.Notification;
using DigitalWallet.Application.Interfaces.Repositories;
using DigitalWallet.Application.Interfaces.Services;

namespace DigitalWallet.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(
            Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var notifications = await _notificationRepository.GetByUserIdAsync(userId, pageNumber, pageSize);
                var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);

                return ServiceResult<IEnumerable<NotificationDto>>.Success(notificationDtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<NotificationDto>>.Failure(
                    $"Error retrieving notifications: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                await _notificationRepository.MarkAsReadAsync(notificationId);
                return ServiceResult<bool>.Success(true, "Notification marked as read");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Error marking notification as read: {ex.Message}");
            }
        }

        public async Task<ServiceResult<int>> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                var count = await _notificationRepository.GetUnreadCountAsync(userId);
                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Failure($"Error getting unread count: {ex.Message}");
            }
        }
    }
}