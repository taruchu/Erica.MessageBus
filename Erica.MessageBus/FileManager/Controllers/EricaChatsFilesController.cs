using System;
using System.Threading.Tasks;
using EricaChats.DataAccess.Services.SQL;
using FileManager.Interfaces.NoSql;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SharedInterfaces.Interfaces.EricaChats;
using SharedInterfaces.Models.DTO;


//TODO: Add O-Auth 

namespace FileManager.Controllers
{   
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class EricaChatsFilesController : Controller
    {
        private EricaChats_DBContext _ericaChats_DBContext { get; set; }
        private IEricaChatsFilesRepository _ericaChatsFilesRepository { get; set; }

        public EricaChatsFilesController(EricaChats_DBContext ericaChats_DBContext, IEricaChatsFilesRepository ericaChatsFilesRepository)
        {
            _ericaChats_DBContext = ericaChats_DBContext;
            _ericaChatsFilesRepository = ericaChatsFilesRepository;
        } 
         
        [HttpGet("GetFileMetaDataByChannel/{channelId}", Name ="GetFileMetaDataByChannelId")]
        public JsonResult GetFileMetaDataByChannel(int channelId)
        { 
            try
            {
                var result = _ericaChats_DBContext.GetFileMetaDataList(channelId);
                 
                return new JsonResult(result, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            catch (Exception ex)
            {
                 
                throw new ApplicationException(ex.Message, ex);
            }
        }
         
        [HttpGet("DownloadFile/{fileNameGuid}", Name = "DownloadAFile")]
        public async Task<JsonResult> DownloadFile(string fileNameGuid)
        { 
            try
            {
                IEricaChats_FileDTO result = new EricaChats_FileDTO();
                var fileBytes = await _ericaChatsFilesRepository.DownloadFileAsBytesAsync(fileNameGuid);
                result.FileNameGuid = fileNameGuid;
                result.FileBytesAsAsBase64String = Convert.ToBase64String(fileBytes);

                return new JsonResult(result, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            catch (Exception ex)
            {

                throw new ApplicationException(ex.Message, ex);
            }
        }
        
        [HttpPost("UploadFile", Name = "UploadNewFile")]
        public async Task<JsonResult> UploadFile(IEricaChats_FileDTO ericaChats_FileDTO)
        { 
            try
            { 
                var result = await _ericaChatsFilesRepository.UploadFileFromBytesAsync(ericaChats_FileDTO.FileNameGuid, Convert.FromBase64String(ericaChats_FileDTO.FileBytesAsAsBase64String));
                
                return new JsonResult(result, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
            }
            catch (Exception ex)
            {

                throw new ApplicationException(ex.Message, ex);
            }
        }


    }
}
