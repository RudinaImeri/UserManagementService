using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Domain.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.Core.Service
{
    public interface IMapperService
    {
        public ApplicationUser MapUserForUpdateEntityToDto(UpdateDto userEntity);
        public ApplicationUser MapUserForCreationDtoToEntity(RegisterDto userEntity);
        public ApplicationUser MapUserForAuthenticationEntityToDto(UserForAuthenticationDto userEntity);
    }
    public class MapperService : IMapperService
    {
        private readonly IMapper _mapper;
        public MapperService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public ApplicationUser MapUserForAuthenticationEntityToDto(UserForAuthenticationDto userForAuthenticationDto)
        {
            return _mapper.Map<ApplicationUser>(userForAuthenticationDto);
        }

        public ApplicationUser MapUserForCreationDtoToEntity(RegisterDto userForCreationDto)
        {
            return _mapper.Map<ApplicationUser>(userForCreationDto);
        }

        public ApplicationUser MapUserForUpdateEntityToDto(UpdateDto userForUpdateDto)
        {
            return _mapper.Map<ApplicationUser>(userForUpdateDto);
        }
    }
}
