====================================================
Day 02 — AWS S3 (CLI + C# Example)
====================================================

GOAL:
-----
Learn to:
1. Create an S3 bucket via CLI
2. Upload, download, and list objects via CLI
3. Perform the same operations in C# using AWS SDK

----------------------------------------------------
PART 1 — AWS CLI Commands
----------------------------------------------------
💡 NOTE:
- AWS CLI commands work in BOTH Command Prompt (cmd.exe) and PowerShell.
- In Command Prompt: Use `^` for line continuation.
- In PowerShell: Use `` ` `` (backtick) for line continuation.

----------------------------------------------------
1️⃣ Create a bucket
----------------------------------------------------
# Command Prompt (cmd.exe)
aws s3api create-bucket ^
    --bucket <your-unique-bucket-name> ^
    --region ap-south-1 ^
    --create-bucket-configuration LocationConstraint=ap-south-1 ^
    --profile <your-profile-name>

# PowerShell
aws s3api create-bucket `
    --bucket <your-unique-bucket-name> `
    --region ap-south-1 `
    --create-bucket-configuration LocationConstraint=ap-south-1 `
    --profile <your-profile-name>

Example:
aws s3api create-bucket ^
    --bucket abdul-aws-s3-demo-20250809 ^
    --region ap-south-1 ^
    --create-bucket-configuration LocationConstraint=ap-south-1 ^
    --profile aws-csharp

----------------------------------------------------
2️⃣ List all buckets
----------------------------------------------------
CMD:
aws s3api list-buckets --profile <your-profile-name>

PowerShell:
aws s3api list-buckets --profile <your-profile-name>

Example:
aws s3api list-buckets --profile aws-csharp

----------------------------------------------------
3️⃣ Upload a file to bucket
----------------------------------------------------
CMD:
aws s3 cp localfile.txt s3://<your-bucket-name>/ --profile <your-profile-name>

PowerShell:
aws s3 cp localfile.txt s3://<your-bucket-name>/ --profile <your-profile-name>

Example:
aws s3 cp test_upload.txt s3://abdul-aws-s3-demo-20250809/ --profile aws-csharp

----------------------------------------------------
4️⃣ List files in a bucket
----------------------------------------------------
CMD:
aws s3 ls s3://<your-bucket-name>/ --profile <your-profile-name>

PowerShell:
aws s3 ls s3://<your-bucket-name>/ --profile <your-profile-name>

Example:
aws s3 ls s3://abdul-aws-s3-demo-20250809/ --profile aws-csharp

----------------------------------------------------
5️⃣ Download a file from bucket
----------------------------------------------------
CMD:
aws s3 cp s3://<your-bucket-name>/<file-key> localfile.txt --profile <your-profile-name>

PowerShell:
aws s3 cp s3://<your-bucket-name>/<file-key> localfile.txt --profile <your-profile-name>

Example:
aws s3 cp s3://abdul-aws-s3-demo-20250809/test_upload.txt downloaded.txt --profile aws-csharp

----------------------------------------------------
6️⃣ Delete a file from bucket
----------------------------------------------------
CMD:
aws s3 rm s3://<your-bucket-name>/<file-key> --profile <your-profile-name>

PowerShell:
aws s3 rm s3://<your-bucket-name>/<file-key> --profile <your-profile-name>

Example:
aws s3 rm s3://abdul-aws-s3-demo-20250809/test_upload.txt --profile aws-csharp

----------------------------------------------------
7️⃣ Delete bucket (must be empty first)
----------------------------------------------------
CMD:
aws s3api delete-bucket --bucket <your-bucket-name> --profile <your-profile-name>

PowerShell:
aws s3api delete-bucket --bucket <your-bucket-name> --profile <your-profile-name>

Example:
aws s3api delete-bucket --bucket abdul-aws-s3-demo-20250809 --profile aws-csharp

----------------------------------------------------
PART 2 — C# S3 Example
----------------------------------------------------
Requirements:
-------------
dotnet add package AWSSDK.Core
dotnet add package AWSSDK.S3

Namespaces:
-----------
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime.CredentialManagement;

Code:
-----
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
    }
}

----------------------------------------------------
NOTES:
------
1. Always choose unique bucket names (global namespace)
2. Avoid hardcoding keys — use AWS CLI profiles instead
3. Test CLI before C# to ensure credentials are correct
4. Region must match the bucket's region
5. Bucket must be empty before deleting

====================================================
END OF DAY 02
====================================================
