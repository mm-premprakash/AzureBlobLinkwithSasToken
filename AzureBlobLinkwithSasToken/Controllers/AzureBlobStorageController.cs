using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureBlobLinkwithSasToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureBlobStorageController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AzureBlobStorageController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetBlobFileLink")]
        public string GetBlobFileLink(string blobFileName)
        {
            var blobFileLink = GenerateSasToken(blobFileName);
            return blobFileLink;
        }
        private string GenerateSasToken(string blobname)
        {
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(
                new Uri(_configuration["AzureBlob:ServiceUri"] ?? ""),
                new StorageSharedKeyCredential(_configuration["AzureBlob:StorageName"], _configuration["AzureBlob:AccountKey"])
            );

            // Get the BlobContainerClient and BlobClient
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_configuration["AzureBlob:StorageName"]);
            var blobClient = blobContainerClient.GetBlobClient(blobname); // blobname should containg folder path if there is any and filename

            // Define the SAS token parameters
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _configuration["AzureBlob:StorageName"],
                BlobName = blobname,
                Resource = "b", // "b" stands for blob
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1) // Set token expiry time
            };

            // Specify the permissions for the SAS token
            sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write | BlobSasPermissions.Tag);

            // Generate the SAS token
            var sasToken = sasBuilder.ToSasQueryParameters(
                new StorageSharedKeyCredential(blobServiceClient.AccountName, _configuration["AzureBlob:AccountKey"])
            ).ToString();

            // Combine the blob URI and SAS token
            var blobUriWithSas = $"{blobClient.Uri}?{sasToken}";

            return blobUriWithSas;
        }
    }
}
