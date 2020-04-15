using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ATGSaveGameManager.Azure
{
    public class BlobStorageService
    {
        string accessKey = string.Empty;

        public BlobStorageService(string key)
        {
            accessKey = key;
        }

        public string UploadFileToBlob(string strFileName, string hash, byte[] fileData, string fileMimeType)
        {
            try
            {
                var _task = Task.Run(() => UploadFileToBlobAsync(strFileName, hash, fileData, fileMimeType));
                _task.Wait();
                string fileUrl = _task.Result;
                return fileUrl;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ConcurrentDictionary<string, FileInfoModel> GetFiles()
        {
            try
            {
                var _task = Task.Run(() => GetFilesAync());
                _task.Wait();
              
                return _task.Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FileInfoModel DownloadFile(string key, string path)
        {
            try
            {
                var _task = Task.Run(() => DownloadFileAsync(key, path));
                _task.Wait();

                return _task.Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<FileInfoModel> DownloadFileAsync(string key, string path)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(accessKey);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            string strContainerName = "savegames";
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(strContainerName);
            if (await cloudBlobContainer.CreateIfNotExistsAsync())
            {
                await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }

            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(key);

            if (await cloudBlockBlob.ExistsAsync())
            {
                await cloudBlockBlob.DownloadToFileAsync(path, FileMode.Create);
                var fileModel = new FileInfoModel
                {
                    Name = cloudBlockBlob.Name,
                    Md5 = cloudBlockBlob.Properties.ContentMD5,
                    LastModified = cloudBlockBlob.Properties.LastModified
                };

                return fileModel;
            }
            return null;
        }

        private async Task<ConcurrentDictionary<string, FileInfoModel>> GetFilesAync()
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(accessKey);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            string strContainerName = "savegames";
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(strContainerName);
            if (await cloudBlobContainer.CreateIfNotExistsAsync())
            {
                await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }
            var result = await cloudBlobContainer.ListBlobsSegmentedAsync(null);

            BlobContinuationToken continuationToken = null;
            CloudBlob blob;
            ConcurrentDictionary<string, FileInfoModel> files = new ConcurrentDictionary<string, FileInfoModel>();
            try
            {
                // Call the listing operation and enumerate the result segment.
                // When the continuation token is null, the last segment has been returned
                // and execution can exit the loop.
                do
                {
                    BlobResultSegment resultSegment = await cloudBlobContainer.ListBlobsSegmentedAsync(continuationToken);

                    foreach (var blobItem in resultSegment.Results)
                    {
                        // A flat listing operation returns only blobs, not virtual directories.
                        blob = (CloudBlob)blobItem;

                        var fileModel = new FileInfoModel
                        {
                            Name = blob.Name,
                            Md5 = blob.Properties.ContentMD5,
                            LastModified = blob.Properties.LastModified
                        };
                        files.TryAdd(fileModel.Name, fileModel);

                        // Write out some blob properties.
                        Console.WriteLine("Blob name: {0}", blob.Name);
                    }

                    Console.WriteLine();

                    // Get the continuation token and loop until it is null.
                    continuationToken = resultSegment.ContinuationToken;

                } while (continuationToken != null);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }

            return files;
        }

        private async Task<string> UploadFileToBlobAsync(string strFileName, string hash, byte[] fileData, string fileMimeType)
        {
            try
            {
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(accessKey);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                string strContainerName = "savegames";
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(strContainerName);

                if (await cloudBlobContainer.CreateIfNotExistsAsync())
                {
                    await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                }

                if (strFileName != null && fileData != null)
                {
                    CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(strFileName);
                    if (await cloudBlockBlob.ExistsAsync())
                    {
                        if (hash != cloudBlockBlob.Properties.ContentMD5)
                        {
                            return await Upload(cloudBlockBlob, fileMimeType, fileData);
                        }
                    }
                    else
                    {
                        return await Upload(cloudBlockBlob, fileMimeType, fileData);
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private async Task<string> Upload(CloudBlockBlob cloudBlockBlob, string fileMimeType, byte[] fileData)
        {
            cloudBlockBlob.Properties.ContentType = fileMimeType;
            await cloudBlockBlob.UploadFromByteArrayAsync(fileData, 0, fileData.Length);
            return cloudBlockBlob.Uri.AbsoluteUri;
        }
    }
}