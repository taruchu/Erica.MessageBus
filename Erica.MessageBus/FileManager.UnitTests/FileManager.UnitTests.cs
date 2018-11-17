using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileManager.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestUploadFile()
        {
            //Set up the request File DTO and it's contents

            //Call the File Manager to perform an upload

            //Check the response to ensure it was successful
        }

        [TestMethod]
        public void TestDownloadFile()
        {
            //Set up the request File DTO and it's contents

            //Call the File Manager to perform an upload

            //Check the response to ensure it was successful

            //Now use the File Guid to Download the file using the File Manager

            //Compare the response File DTO contents to the request File DTO contents. They should be identical.
        }

        [TestMethod]
        public void TestGetFileMetaDataByChannel()
        {
            //Call the File Manager giving it a valid ChannelId

            //Check the response list and UnMarshall it into a List of File DTO objects.

            //Verify the list
        }
    }
}
