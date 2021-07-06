using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Helpers;

namespace WebAPI.Controllers
{
    [Route("api/greetings")]
    [ApiController]
    public class GreetingsController : ControllerBase
    {
        [HttpGet("{name}")]
        public ActionResult<string> GetGreeting(string name)
        {
            return $"Hello, {name}";
        }

        [HttpGet("async/{name}")]
        public async Task<ActionResult<string>> GetGreetingAsync(string name)
        {
            var waitingTime = RandomGen.NextDouble() * 10 + 1;
            await Task.Delay(TimeSpan.FromSeconds(waitingTime));

            //MyAsyncVoidMethod();
            AsyncTaskMethod();
            return $"Hello, {name}";
        }

        [HttpGet("goodbye/{name}")]
        public async Task<ActionResult<string>> GetGoodbye(string name)
        {
            var waitingTime = RandomGen.NextDouble() * 10 + 1;
            await Task.Delay(TimeSpan.FromSeconds(waitingTime));


            return $"Good bye, {name}";
        }

        [HttpGet("asit")]
        public ActionResult<string> GetGreetings()
        {
            return "Hello Asit";
        }

        private async Task AsyncTaskMethod()
        {
            await Task.Delay(1);
            throw new ApplicationException();
        }


        //Anti Pattern - Very dangerous, should be avoid at all cost
        private async void MyAsyncVoidMethod()
        {
            await Task.Delay(1);
            throw new ApplicationException();
        }
    }
}
