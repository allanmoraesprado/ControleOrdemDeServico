using OsService.Domain.Enums;

namespace OsService.ApiService.V1.ServiceOrders.Dtos;

public sealed record ChangeServiceOrderStatusDto(ServiceOrderStatus Status);