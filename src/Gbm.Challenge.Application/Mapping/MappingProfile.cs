using AutoMapper;
using Gbm.Challenge.Domain.Entities;
using Gbm.Challenge.Domain.Models.DTOs;

namespace Gbm.Challenge.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderDTO>().ReverseMap();
    }
}
