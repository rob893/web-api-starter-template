using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ProjectTemplate.Data.Repositories;
using ProjectTemplate.Models.QueryParameters;
using ProjectTemplate.Models.DTOs;
using ProjectTemplate.Core;
using ProjectTemplate.Models.Responses;
using ProjectTemplate.Models.Domain;

namespace ProjectTemplate.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository userRepository;
        private readonly IMapper mapper;


        public UsersController(UserRepository userRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<UserForReturnDto>>> GetUsersAsync([FromQuery] CursorPaginationParameters searchParams)
        {
            var users = await userRepository.SearchAsync(searchParams);
            var paginatedResponse = CursorPaginatedResponse<UserForReturnDto>.CreateFrom(users, mapper.Map<IEnumerable<UserForReturnDto>>);

            return Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = "GetUserAsync")]
        public async Task<ActionResult<UserForReturnDto>> GetUserAsync(int id)
        {
            var user = await userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound(new ProblemDetailsWithErrors($"User with id {id} does not exist."));
            }

            var userToReturn = mapper.Map<UserForReturnDto>(user);

            return Ok(userToReturn);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("roles")]
        public async Task<ActionResult<CursorPaginatedResponse<RoleForReturnDto>>> GetRolesAsync([FromQuery] CursorPaginationParameters searchParams)
        {
            var roles = await userRepository.GetRolesAsync(searchParams);
            var paginatedResponse = CursorPaginatedResponse<RoleForReturnDto>.CreateFrom(roles, mapper.Map<IEnumerable<RoleForReturnDto>>);

            return Ok(paginatedResponse);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("{id}/roles")]
        public async Task<ActionResult<UserForReturnDto>> AddRolesAsync(int id, [FromBody] RoleEditDto roleEditDto)
        {
            if (roleEditDto.RoleNames == null || roleEditDto.RoleNames.Length == 0)
            {
                return BadRequest(new ProblemDetailsWithErrors("At least one role must be specified.", 400, Request));
            }

            var user = await userRepository.GetByIdAsync(id);
            var roles = await userRepository.GetRolesAsync();
            var userRoles = user.UserRoles.Select(ur => ur.Role.Name.ToUpper()).ToHashSet();
            var selectedRoles = roleEditDto.RoleNames.Select(role => role.ToUpper()).ToHashSet();

            var rolesToAdd = roles.Where(role =>
            {
                var upperName = role.Name.ToUpper();
                return selectedRoles.Contains(upperName) && !userRoles.Contains(upperName);
            });

            if (rolesToAdd.Count() == 0)
            {
                return Ok(mapper.Map<UserForReturnDto>(user));
            }

            user.UserRoles.AddRange(rolesToAdd.Select(role => new UserRole
            {
                Role = role
            }));

            var success = await userRepository.SaveAllAsync();

            if (!success)
            {
                return BadRequest(new ProblemDetailsWithErrors("Failed to add roles.", 400, Request));
            }

            var userToReturn = mapper.Map<UserForReturnDto>(user);

            return Ok(userToReturn);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete("{id}/roles")]
        public async Task<ActionResult<UserForReturnDto>> RemoveRolesAsync(int id, [FromBody] RoleEditDto roleEditDto)
        {
            if (roleEditDto.RoleNames == null || roleEditDto.RoleNames.Length == 0)
            {
                return BadRequest(new ProblemDetailsWithErrors("At least one role must be specified.", 400, Request));
            }

            var user = await userRepository.GetByIdAsync(id);
            var roles = await userRepository.GetRolesAsync();
            var userRoles = user.UserRoles.Select(ur => ur.Role.Name.ToUpper()).ToHashSet();
            var selectedRoles = roleEditDto.RoleNames.Select(role => role.ToUpper()).ToHashSet();

            var roleIdsToRemove = roles.Where(role =>
            {
                var upperName = role.Name.ToUpper();
                return selectedRoles.Contains(upperName) && userRoles.Contains(upperName);
            }).Select(role => role.Id).ToHashSet();

            if (roleIdsToRemove.Count() == 0)
            {
                return Ok(mapper.Map<UserForReturnDto>(user));
            }

            user.UserRoles.RemoveAll(ur => roleIdsToRemove.Contains(ur.RoleId));
            var success = await userRepository.SaveAllAsync();

            if (!success)
            {
                return BadRequest(new ProblemDetailsWithErrors("Failed to remove roles.", 400, Request));
            }

            var userToReturn = mapper.Map<UserForReturnDto>(user);

            return Ok(userToReturn);
        }
    }
}