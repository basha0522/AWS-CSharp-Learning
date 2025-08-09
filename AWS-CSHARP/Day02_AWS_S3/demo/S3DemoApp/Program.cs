using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime;
using System.Globalization;

class Program
{
    static async Task Main(string[] args)
    {
        var bucketName = "abdul-aws-s3-demo-20250809";// s3bucket name
        var awsProfileName = "aws-csharp"; // AWS CLI Profile
        var region = Amazon.RegionEndpoint.APSouth1;
        var chain = new CredentialProfileStoreChain();
        // reading file from S3 bucket
        
        if (chain.TryGetAWSCredentials(awsProfileName, out var aWSCredentials))
        {
            var S3Client = new AmazonS3Client(aWSCredentials, region);
            Console.WriteLine($"Listing objects in bucket: {bucketName}");
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName
            };

            var response = await S3Client.ListObjectsV2Async(request);
            foreach (var obj in response.S3Objects)
            {
                Console.WriteLine($" - {obj.Key} (size: {obj.Size} bytes)");
            }
        }
        else
        {
            Console.WriteLine($"Profile '{awsProfileName}' not found.");
        }
        
        // uploading and downloading from S3 bucket
        if (!chain.TryGetAWSCredentials(awsProfileName, out var aWSCredentialsreturn))
        {
            Console.WriteLine($"Profile '{awsProfileName}' not found.");
            return;
        }
        else
        {
            var S3Client = new AmazonS3Client(aWSCredentialsreturn, region);
            // 1️⃣ List files
            await ListFilesAsync(S3Client, bucketName);

            // Uploading file
            string localFilePath = "test_upload.txt";
            File.WriteAllText(localFilePath, "Upload from console C# app..");
            await UploadFileAsync(S3Client, bucketName, localFilePath);

            // downloading file
            string downloadFilePath = "downloaded_test_upload.txt";
            await DownloadFileAsync(S3Client, bucketName, "test_upload.txt", downloadFilePath);

            Console.WriteLine("All file operations are completed...!");
        }
        //till here
    }
    // 🔹 Method to list files
    static async Task ListFilesAsync(AmazonS3Client s3Client, string bucketName)
    {
        Console.WriteLine($"\nListing objects in bucket: {bucketName}");
        var request = new ListObjectsV2Request { BucketName = bucketName };
        var response = await s3Client.ListObjectsV2Async(request);

        foreach (var obj in response.S3Objects)
        {
            Console.WriteLine($" - {obj.Key} (size: {obj.Size} bytes)");
        }
    }
    static async Task UploadFileAsync(AmazonS3Client s3Client, string bucketName, string filePath)
    {
        Console.WriteLine($"\nUploading file: {filePath}");
        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = Path.GetFileName(filePath),
            FilePath = filePath
        };
        await s3Client.PutObjectAsync(putRequest);
        Console.WriteLine("Upload Complete...");
    }
    static async Task DownloadFileAsync(AmazonS3Client s3Client, string bucketName, string key, string destinationPath)
    {
        Console.WriteLine($"\ndownload file: {key}");
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };
        using var objResponse = await s3Client.GetObjectAsync(request);
        await objResponse.WriteResponseStreamToFileAsync(destinationPath, false, default);
        Console.WriteLine("Download Complete..!");
    }
}