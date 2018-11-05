using FileManager.Interfaces.NoSql;
using FileManager.Models.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager.Services.NoSql
{
    public class EricaChatsFilesDBContext : IEricaChatsFilesRepository
    {
        private readonly IMongoDatabase _database = null;
        private readonly IGridFSBucket _gridFSBucket = null;

        public EricaChatsFilesDBContext(IOptions<Settings> settings)
        {
            //TODO Add logging

            var client = new MongoClient(settings.Value.ConnectionString);
            if (client == null)
                throw new ApplicationException("Could not connect to client.");
            else
                _database = client.GetDatabase(settings.Value.Database); 
                

            if (_database == null)
                throw new ApplicationException("Could not find the collection.");
            else
            {
                _gridFSBucket = new GridFSBucket(_database, new GridFSBucketOptions
                {
                    BucketName = "EricaChatsFilesGridFSBucket",
                    ChunkSizeBytes = 1048576, // 1 MB,
                    WriteConcern = WriteConcern.WMajority,
                    ReadPreference = ReadPreference.Secondary
                });
            }
        }

        public async Task<byte[]> DownloadFileAsBytesAsync(string fileNameGUID)
        {
            byte[] bytes = await _gridFSBucket.DownloadAsBytesByNameAsync(fileNameGUID);
            return bytes; 
        }

        public async Task<bool> UploadFileFromBytesAsync(string fileNameGUID, byte[] fileBytes)
        {
            //TODO: Add Metadata
            ObjectId Id = await _gridFSBucket.UploadFromBytesAsync(fileNameGUID, fileBytes);
            return (Id != null);
        }
    }
}
