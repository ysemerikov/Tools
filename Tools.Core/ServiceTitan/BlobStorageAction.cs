using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Tools.Core.ServiceTitan
{
    public class BlobStorageAction : IAction
    {
        public Task Do()
        {
            var source = GetContainer(Secrets.Azure.Titanblobs, "p-nicholsonplumbingheatingairconditioningc-626219346");
            var destination = GetContainer(Secrets.Azure.Titanblobs, "p-nicholsonplumbingheatingandairconditioning-621794657");

            var folder = "Call/Recording";
            return Copy(
                source.GetDirectoryReference(folder),
                destination.GetDirectoryReference(folder)
            );
        }

        private static async Task Copy(CloudBlobDirectory source, CloudBlobDirectory destination)
        {
            var sourceFiles = await GetAllFiles(source);
            var destinationFiles = await GetAllFiles(destination);
            Console.WriteLine($"{sourceFiles.Count} == {destinationFiles.Count}");
        }

        private static async Task<List<CloudBlockBlob>> GetAllFiles(CloudBlobDirectory source)
        {
            var result = new List<CloudBlockBlob>(15000);
            var token = default(BlobContinuationToken);
            do
            {
                var s = await source.ListBlobsSegmentedAsync(token);
                token = s.ContinuationToken;

                foreach (var blobItem in s.Results)
                {
                    if (!(blobItem is CloudBlockBlob cloudBlockBlob))
                        throw new Exception();
                    result.Add(cloudBlockBlob);
                }
            } while (token != null);

            return result;
        }

        private CloudBlobContainer GetContainer(string connectionString, string containerName)
        {
            var a = CloudStorageAccount.Parse(connectionString);
            return new CloudBlobClient(a.BlobEndpoint, a.Credentials).GetContainerReference(containerName);
        }
    }
}