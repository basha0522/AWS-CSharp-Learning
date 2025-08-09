using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime.CredentialManagement;

namespace AwsS3Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var bucketName = "abdul-aws-s3-demo-20250809"; // change this
            var profileName = "aws-csharp";                // your AWS CLI profile
            var region = Amazon.RegionEndpoint.APSouth1;

            // Load AWS credentials from profile
            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetAWSCredentials(profileName, out var awsCredentials))
            {
                Console.WriteLine($"Profile '{profileName}' not found.");
                return;
            }

            var s3Client = new AmazonS3Client(awsCredentials, region);

            // 1️⃣ List files
            await ListFilesAsync(s3Client, bucketName);

            // 2️⃣ Upload a file
            string localFilePath = "test_upload.txt";
            File.WriteAllText(localFilePath, "Hello from Day 02 C# upload!");
            await UploadFileAsync(s3Client, bucketName, localFilePath);

            // 3️⃣ Download a file
            string downloadFilePath = "downloaded_test_upload.txt";
            await DownloadFileAsync(s3Client, bucketName, "test_upload.txt", downloadFilePath);

            // 4️⃣ Delete a file
            await DeleteFileAsync(s3Client, bucketName, "test_upload.txt");

            // 5️⃣ Multipart upload (for large files)
            string bigFilePath = "big_file_test.txt";
            File.WriteAllText(bigFilePath, new string('A', 6 * 1024 * 1024)); // ~6MB
            await MultipartUploadAsync(s3Client, bucketName, bigFilePath);

            Console.WriteLine("✅ All S3 operations completed.");
        }

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
            Console.WriteLine("✅ Upload complete.");
        }

        static async Task DownloadFileAsync(AmazonS3Client s3Client, string bucketName, string key, string destinationPath)
        {
            Console.WriteLine($"\nDownloading file: {key}");
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using var response = await s3Client.GetObjectAsync(request);
            await response.WriteResponseStreamToFileAsync(destinationPath, false, default);
            Console.WriteLine("✅ Download complete.");
        }
        static async Task DeleteFileAsync(AmazonS3Client s3Client, string bucketName, string key)
        {
            Console.WriteLine($"\nDeleting file: {key}");
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };
            await s3Client.DeleteObjectAsync(request);
            Console.WriteLine("✅ Delete complete.");
        }

        static async Task MultipartUploadAsync(AmazonS3Client s3Client, string bucketName, string filePath)
        {
            Console.WriteLine($"\nMultipart uploading file: {filePath}");
            var keyName = Path.GetFileName(filePath);
            var initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = keyName
            };

            var initResponse = await s3Client.InitiateMultipartUploadAsync(initiateRequest);
            var uploadId = initResponse.UploadId;
            var partETags = new List<PartETag>();
            const int partSize = 5 * 1024 * 1024; // 5 MB

            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                long filePosition = 0;
                for (int partNumber = 1; filePosition < fileStream.Length; partNumber++)
                {
                    var uploadRequest = new UploadPartRequest
                    {
                        BucketName = bucketName,
                        Key = keyName,
                        UploadId = uploadId,
                        PartNumber = partNumber,
                        PartSize = Math.Min(partSize, fileStream.Length - filePosition),
                        InputStream = fileStream
                    };

                    var uploadResponse = await s3Client.UploadPartAsync(uploadRequest);
                    partETags.Add(new PartETag(partNumber, uploadResponse.ETag));
                    filePosition += partSize;
                }

                var completeRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    UploadId = uploadId,
                    PartETags = partETags
                };

                await s3Client.CompleteMultipartUploadAsync(completeRequest);
                Console.WriteLine("✅ Multipart upload complete.");
            }
            catch
            {
                await s3Client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    UploadId = uploadId
                });
                Console.WriteLine("❌ Multipart upload aborted.");
                throw;
            }
        }
    }
}