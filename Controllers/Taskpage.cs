using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.IdentityModel.Tokens.Jwt;

namespace Project.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Taskcontroller : ControllerBase
    {
        public MyDbContext _context;
        public IHttpContextAccessor _http;
        public string tokenuserId;
        private string username;
        public Taskcontroller(MyDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _http = httpContextAccessor;

            var token = _http.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Accessing claims
            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name"); // jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email"); // jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            // Access the values from the claims
            username = usernameClaim?.Value;
            if (userIdClaim == null){
                throw new Exception("Null");
            }
            tokenuserId = userIdClaim.Value;
        }

        [HttpGet("getTasks")]
        public IActionResult GetTasks()
        {
            var tasklist = _context.tasks.ToList();
            return Ok(tasklist);
        }

        [HttpGet("getUserTask")]
        [Authorize]


        public IActionResult GetUserTask()
        {
            
            var userTasks = _context.tasks.Where(task => task.userId.ToString() == tokenuserId && task.iscompleted==false).OrderByDescending(i=>i.taskId).ToList();
            if (userTasks != null)
            {
                return Ok(userTasks);
            }
           
            else
            {
                return BadRequest("No tasks to show. It looks like you have the day off!");
            }
            


        }

        [HttpPost("insertUserTask")]
        [Authorize]
        public IActionResult Post([FromBody] tasks obj){
            if (obj == null)
                return BadRequest();
            if (tokenuserId == null)
                return BadRequest();
            obj.userId = int.Parse(tokenuserId);
            obj.createDate=DateTime.Now;
            _context.tasks.Add(obj);
            _context.SaveChanges();
            return Ok(obj);
        }

        [HttpDelete("deleteUserTask")]
        [Authorize]
        public IActionResult Delete(int id){
            var delete=_context.tasks.Find(id);

            _context.tasks.Remove(delete);
            _context.SaveChanges();
            return Ok(delete);
        }

        [HttpPut("updateTask")]
        [Authorize]
        public IActionResult update(int id, [FromBody] updateTask updateTaskobj){
            var record=_context.tasks.Find(id);
            record.title=updateTaskobj.title;
            record.description=updateTaskobj.description;
            record.dueDate=updateTaskobj.dueDate;
            _context.SaveChanges();
            return Ok(record);
        }

        [HttpPut("completeUserTask")]
        [Authorize]
        public IActionResult complete(int id){
            var record=_context.tasks.Find(id);
            record.iscompleted=true;
            _context.SaveChanges();
            return Ok(record.iscompleted);
        }

        [HttpPut("undoCompletedTask")]
        [Authorize]
        public IActionResult undo(int id){
             var record=_context.tasks.Find(id);
            record.iscompleted=false;
            _context.SaveChanges();
            return Ok(record.iscompleted);
        }

        [HttpGet("getUserCompletedtask")]
        [Authorize]
        public IActionResult getUserCompletedTask(){
            var completedTasks = _context.tasks.Where(task => task.userId.ToString() == tokenuserId && task.iscompleted==true).OrderByDescending(i=>i.taskId).ToList();
            if(completedTasks!=null){
                return Ok(completedTasks);
            }
            else{
                return BadRequest("You have not completed any tasks");
            }
        }
        
    }
}
