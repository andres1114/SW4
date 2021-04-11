using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase {
        private readonly DataContext context;

        public UsersController(DataContext context) {
            this.context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<AppUser>> GetUsers() {
            var users = this.context.Users.ToList();
            return users;
        }
        [HttpGet("{id}")]
        public ActionResult<AppUser> GetUsers(int id) {
            var user = this.context.Users.Find(id);
            return user;
        }
    }
}
