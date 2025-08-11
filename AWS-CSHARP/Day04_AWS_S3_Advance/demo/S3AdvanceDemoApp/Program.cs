using System;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using System.Threading.Tasks;
namespace S3AdvanceDemoApp
{
    class S3AdvanceDemoApp
    {
        public static async Task Main(string[] args)
        {
            // easiest way 
            //  — TransferUtility (recommended)This is the recommended simple approach — the SDK handles splitting, parallelism, retries
            //await AWSUploadMultipartSelfList();

            //manual multipart (deep control)
            //If you want to control each part (e.g., upload parts from different machines or have special chunking), 
            // use the Initiate/UploadPart/Complete flow
            await AWSManualUploadMultiPart();
        }
        static async Task AWSUploadMultipartSelfList()
        {
            var chain = new CredentialProfileStoreChain();
            chain.TryGetAWSCredentials("aws-csharp", out var awscred);
            Console.WriteLine(awscred);
            using var s3 = new AmazonS3Client(awscred, Amazon.RegionEndpoint.APSouth1);
            var filePath = "D:/Learning/Angular14.zip";
            var awsbucketname = "abdul-aws-s3-demo-20250809";
            var objTransferUti = new TransferUtility(s3);
            await objTransferUti.UploadAsync(filePath, awsbucketname);
            Console.WriteLine("File Upload Success..!");
        }
        static async Task AWSManualUploadMultiPart()
        {
            var chain = new CredentialProfileStoreChain();
            chain.TryGetAWSCredentials("aws-csharp", out var awscred);
            using var s3 = new AmazonS3Client(awscred, Amazon.RegionEndpoint.APSouth1);
            string keyName = Path.GetFileName("D:/Learning/Angular14.zip");
            var initRequest = new InitiateMultipartUploadRequest
            {
                BucketName = "abdul-aws-s3-demo-20250809",
                Key = keyName
            };
            var initResponse = await s3.InitiateMultipartUploadAsync(initRequest);
            var uploadId = initResponse.UploadId;
            Console.WriteLine($"Upload ID: {uploadId}");
            var partETags = new List<PartETag>();
            const int partSize = 5 * 1024 * 1024;
            try
            {
                using var objfs = new FileStream("D:/Learning/Angular14.zip", FileMode.Open, FileAccess.Read);
                int partNumber = 1;
                byte[] buffer = new byte[partSize];
                int bytesRead;
                while ((bytesRead = await objfs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    using var ms = new MemoryStream(buffer, 0, bytesRead);
                    var objUploadPartRequest = new UploadPartRequest
                    {
                        BucketName = "abdul-aws-s3-demo-20250809",
                        Key = keyName,
                        UploadId = uploadId,
                        PartNumber = partNumber,
                        InputStream = ms,
                        PartSize = bytesRead
                    };
                    var objUploadPartResponse = await s3.UploadPartAsync(objUploadPartRequest);
                    partETags.Add(new PartETag(partNumber, objUploadPartResponse.ETag));
                    Console.WriteLine($"Upload Part Number {partNumber}, ETag:{objUploadPartResponse.ETag}");
                    partNumber++;

                    var objCompleteRequest = new CompleteMultipartUploadRequest
                    {
                        BucketName = "abdul-aws-s3-demo-20250809",
                        Key = keyName,
                        UploadId = uploadId,
                        PartETags = partETags
                    };
                    var objCompleteRespone = await s3.CompleteMultipartUploadAsync(objCompleteRequest);
                    Console.WriteLine($"Multipart upload Complete.. {objCompleteRespone.Location}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error.. {ex.Message}");
            }
        }
    }    
}