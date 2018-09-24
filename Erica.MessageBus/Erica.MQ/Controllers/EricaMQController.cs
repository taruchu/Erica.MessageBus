using Erica.MQ.Interfaces.DataTransferObjects;
using Erica.MQ.Models.SQL;
using Erica.MQ.Services.SQL;
using Microsoft.AspNetCore.Mvc;
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
        public JsonResult Post(IEricaMQ_MessageDTO ericaMQ_Message)
        {
            var recipt = _ericaMQ_DBContext.POST(ericaMQ_Message);

            return new JsonResult(recipt, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        [HttpPut]
        public JsonResult Put(IEricaMQ_MessageDTO ericaMQ_Message)
        {
            var recipt = _ericaMQ_DBContext.PUT(ericaMQ_Message);

            return new JsonResult(recipt, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
    }
}