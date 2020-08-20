
using System;
using System.Collections.Generic;
using System.Linq;
using EmployeeService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly EmployeeDBContext _context;

        public TokenController(IConfiguration config, EmployeeDBContext context)
        {
            _configuration = config;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Employees _employeeData)
        {

            if (_employeeData != null && _employeeData.Id > 0 && _employeeData.EmployeeId != null)
            {
                var employee = await GetUser(_employeeData.EmployeeId, _employeeData.Id);

                if (employee != null)
                {
                    //create claims details based on the user information
                    var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("Id", employee.Id.ToString()),
                    new Claim("EmployeeId", employee.EmployeeId),
                    new Claim("Name", employee.Name),
                    new Claim("Address", employee.Address),
                    new Claim("Role", employee.Role),
                    new Claim("Department", employee.Department),
                    new Claim("SkillSets", employee.SkillSets),
                    new Claim("BirthDate", employee.BirthDate.ToString()),
                    new Claim("JoinDate", employee.JoiningDate.ToString()),
                    new Claim("IsActive", employee.IsActive.ToString()),
                   };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddDays(1), signingCredentials: signIn);

                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
                else
                {
                    return BadRequest("Invalid credentials");
                }
            }
            else
            {
                return BadRequest();
            }
        }

        private async Task<Employees> GetUser(string eID, int id)
        {
            return await _context.Employees.FirstOrDefaultAsync(u => u.EmployeeId == eID && u.Id == id);
        }
    }
}