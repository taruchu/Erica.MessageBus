using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Erica.MQ.Models.SQL;
using Erica.MQ.Services.SQL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters.Json;
using Newtonsoft.Json;

namespace Erica.MQ.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class EricaMQController : ControllerBase
    {
        private EricaMQ_DBContext _ericaMQ_DBContext { get; set; }

        public EricaMQController(EricaMQ_DBContext ericaMQ_DBContext)
        {
            _ericaMQ_DBContext = ericaMQ_DBContext;
        }

        [HttpPost]
        public JsonResult POST(EricaMQ_Message ericaMQ_Message)
        {
            return new JsonResult(ericaMQ_Message, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto 
            });
        }

        [HttpPut]
        public JsonResult PUT(EricaMQ_Message ericaMQ_Message)
        {
            return new JsonResult(ericaMQ_Message, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
    }
}