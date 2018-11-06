using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EricaChats.DataAccess.Services.SQL;
using Microsoft.AspNetCore.Mvc;
using SharedInterfaces.Interfaces.EricaChats;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FileManager.Controllers
{
    [Route("api/[controller]")]
    public class EricaChatsFilesController : Controller
    {
        private EricaChats_DBContext _ericaChats_DBContext { get; set; }

        public EricaChatsFilesController(EricaChats_DBContext ericaChats_DBContext)
        {
            _ericaChats_DBContext = ericaChats_DBContext;
        }

        public IEricaChats_FileDTO GetFile(string fileNameGuid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEricaChats_FileMetaDataDTO> GetFileMetaByChannel(int channelId)
        {
            throw new NotImplementedException();
        }

        // POST api/<controller>
        [HttpPost]
        public IEricaChats_FileDTO Post(IEricaChats_FileDTO ericaChats_FileDTO)
        {
            throw new NotImplementedException();
        }


    }
}
