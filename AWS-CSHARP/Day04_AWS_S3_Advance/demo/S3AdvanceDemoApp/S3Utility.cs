using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;

namespace Day04_AWSS3
{
    class Program
    {
        private static IAmazonS3 _s3Client;
        private static string _bucketName;

        static async Task Main(string[] args)
        {
            // Load AWS config from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string accessKey = config["AWS:AccessKey"];
            string secretKey = config["AWS:SecretKey"];
            string region = config["AWS:Region"];
            _bucketName = config["AWS:BucketName"];

            _s3Client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.GetBySystemName(region));

            // Call S3 methods
            await CreateBucketAsync();
            await ListBucketsAsync();
            await UploadFileAsync("sample.txt");
            await DownloadFileAsync("sample.txt", "downloaded-sample.txt");
            await GeneratePreSignedURLAsync("sample.txt");
            await EnableVersioningAsync();
            await AddLifecyclePolicyAsync();
            await MultipartUploadAsync("largefile.zip");
            await ListObjectVersionsAsync();
            await DeleteFileAsync("sample.txt");
            await DeleteBucketAsync();
        }

        #region Bucket Operations
        static async Task CreateBucketAsync()
        {
            if (!await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName))
            {
                await _s3Client.PutBucketAsync(new PutBucketRequest
                {
                    BucketName = _bucketName,
                    UseClientRegion = true
                });
                Console.WriteLine($"✅ Bucket '{_bucketName}' created.");
            }
            else
            {
                Console.WriteLine($"ℹ️ Bucket '{_bucketName}' already exists.");
            }
        }

        static async Task ListBucketsAsync()
        {
            var response = await _s3Client.ListBucketsAsync();
            Console.WriteLine("📦 S3 Buckets:");
            foreach (var bucket in response.Buckets)
                Console.WriteLine($" - {bucket.BucketName}");
        }

        static async Task DeleteBucketAsync()
        {
            await _s3Client.DeleteBucketAsync(_bucketName);
            Console.WriteLine($"🗑️ Bucket '{_bucketName}' deleted.");
        }
        #endregion

        #region Object Operations
        static async Task UploadFileAsync(string filePath)
        {
            File.WriteAllText(filePath, "Hello AWS S3!"); // create test file
            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(filePath, _bucketName);
            Console.WriteLine($"📤 File '{filePath}' uploaded.");
        }

        static async Task DownloadFileAsync(string key, string destPath)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.DownloadAsync(destPath, _bucketName, key);
            Console.WriteLine($"📥 File '{key}' downloaded to '{destPath}'.");
        }

        static async Task DeleteFileAsync(string key)
        {
            await _s3Client.DeleteObjectAsync(_bucketName, key);
            Console.WriteLine($"🗑️ File '{key}' deleted from bucket.");
        }
        #endregion

        #region PreSigned URL
        static async Task GeneratePreSignedURLAsync(string key)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(10),
                Verb = HttpVerb.GET
            };
            string url = _s3Client.GetPreSignedURL(request);
            Console.WriteLine($"🔗 PreSigned URL (GET): {url}");
            await Task.CompletedTask;
        }
        #endregion

        #region Multipart Upload
        static async Task MultipartUploadAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllBytes(filePath, new byte[10 * 1024 * 1024]); // create 10MB file
            }

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(filePath, _bucketName);
            Console.WriteLine($"📤 Multipart file '{filePath}' uploaded.");
        }
        #endregion

        #region S3 Management
        static async Task EnableVersioningAsync()
        {
            var request = new PutBucketVersioningRequest
            {
                BucketName = _bucketName,
                VersioningConfig = new S3BucketVersioningConfig { Status = VersionStatus.Enabled }
            };
            await _s3Client.PutBucketVersioningAsync(request);
            Console.WriteLine("✅ Versioning enabled.");
        }

        static async Task ListObjectVersionsAsync()
        {
            var response = await _s3Client.ListVersionsAsync(new ListVersionsRequest
            {
                BucketName = _bucketName
            });

            Console.WriteLine("📜 Object Versions:");
            foreach (var version in response.Versions)
                Console.WriteLine($" - {version.Key} [{version.VersionId}]");
        }

        static async Task AddLifecyclePolicyAsync()
        {
            var request = new PutLifecycleConfigurationRequest
            {
                BucketName = _bucketName,
                Configuration = new LifecycleConfiguration
                {
                    Rules = new System.Collections.Generic.List<LifecycleRule>
                    {
                        new LifecycleRule
                        {
                            Id = "Delete old files after 30 days",
                            Status = LifecycleRuleStatus.Enabled,
                            Expiration = new LifecycleRuleExpiration { Days = 30 },
                            Filter = new LifecycleFilter()
                        }
                    }
                }
            };
            await _s3Client.PutLifecycleConfigurationAsync(request);
            Console.WriteLine("♻️ Lifecycle policy added.");
        }
        #endregion
    }
}
