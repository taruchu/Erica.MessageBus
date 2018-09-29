using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Erica.MQ.Producer.Services.SQL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedInterfaces.Interfaces.EricaChats;

namespace Erica.MQ.Producer.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class EricaChatsController : ControllerBase
    {
        private EricaChats_DBContext _ericaChats_DBContext { get; set; }

        public EricaChatsController(EricaChats_DBContext ericaChats_DbContext)
        {
            _ericaChats_DBContext = ericaChats_DbContext;
        }

        [HttpPost]
        public JsonResult Post(IEricaChats_MessageDTO request)
        {
            //TODO :  add adapter  code here
            /*
             * Inject the dbContext into the adapter contructor
             * Invoke the adapter Execute()
             * Gather the return recipt from the adapter, which should be the EricaMQ message
             * Extract the Data node off of the EricaMQ message and cache it.
             * Send a POST request to the EricaMQ service using the EricaMQ message
             * return the cached data node to the caller as a recipt              * 
             * 
             */

            return new JsonResult("", new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        [HttpPut]
        public JsonResult Put(IEricaChats_MessageDTO request)
        {
            var recipt = _ericaChats_DBContext.PUT(request);

            return new JsonResult(recipt, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
    }
}