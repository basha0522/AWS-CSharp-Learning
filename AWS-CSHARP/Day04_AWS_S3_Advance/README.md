1) Multipart upload — concept & why use it
Short version: break a large object into parts, upload parts independently (can be parallel), then tell S3 to assemble them. Benefits:

Speed: upload parts in parallel.

Reliability / resume: if one part fails, retry only that part.

Required for single objects > 5 GB (AWS requires multipart for very large objects).

Lower risk: network blips don’t force a full re-upload.

High-level flow

InitiateMultipartUpload → S3 returns an UploadId.

Upload parts with UploadPart (part numbers 1..n). Each returns an ETag.

CompleteMultipartUpload with list of {PartNumber, ETag} — S3 assembles final object.

If something goes wrong, AbortMultipartUpload to discard parts and avoid charges.

1A — Quick CLI approach (automatic)
If you simply run:

bash
Copy
Edit
aws s3 cp bigfile.zip s3://your-bucket-name/ --profile aws-csharp
AWS CLI automatically uses multipart upload for large files (default part size and parallelization). So for many uses you don’t need manual steps.

To check in progress / multipart uploads

bash
Copy
Edit
# list active multipart uploads
aws s3api list-multipart-uploads --bucket your-bucket-name --profile aws-csharp
To list parts of an upload (given UploadId & key):

bash
Copy
Edit
aws s3api list-parts --bucket your-bucket-name --key bigfile.zip --upload-id <UploadId> --profile aws-csharp
Abort (CLI):

bash
Copy
Edit
aws s3api abort-multipart-upload --bucket your-bucket-name --key bigfile.zip --upload-id <UploadId> --profile aws-csharp
1B — Manual multipart with aws cli (step-by-step)
Useful for seeing internal flow:

Initiate:

bash
Copy
Edit
aws s3api create-multipart-upload --bucket your-bucket-name --key bigfile.zip --profile aws-csharp
# returns UploadId
Upload part (repeat with PartNumber 1..N, for each slice of file):

bash
Copy
Edit
aws s3api upload-part \
  --bucket your-bucket-name \
  --key bigfile.zip \
  --part-number 1 \
  --body part1.bin \
  --upload-id <UploadId> \
  --profile aws-csharp
After all parts uploaded, collect ETags for each part and complete:
Create parts.json like:

json
Copy
Edit
{
  "Parts": [
    { "ETag": "\"etag1\"", "PartNumber": 1 },
    { "ETag": "\"etag2\"", "PartNumber": 2 }
  ]
}
Then:

bash
Copy
Edit
aws s3api complete-multipart-upload \
  --multipart-upload file://parts.json \
  --bucket your-bucket-name \
  --key bigfile.zip \
  --upload-id <UploadId> \
  --profile aws-csharp
1C — C#: easiest way — TransferUtility (recommended)
This is the recommended simple approach — the SDK handles splitting, parallelism, retries.

csharp
Copy
Edit
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime.CredentialManagement;

var chain = new CredentialProfileStoreChain();
chain.TryGetAWSCredentials("aws-csharp", out var creds);
using var s3 = new AmazonS3Client(creds, Amazon.RegionEndpoint.APSouth1);

var filePath = @"C:\temp\verylarge.zip";
var bucketName = "your-bucket-name";

var transferUtil = new TransferUtility(s3);
await transferUtil.UploadAsync(filePath, bucketName);  // automatically uses multipart for large files
Console.WriteLine("Upload finished (TransferUtility).");
Why use TransferUtility?

Minimal code

Handles part size, parallelism, retries

Good for most apps

1D — C#: manual multipart (deep control)
If you want to control each part (e.g., upload parts from different machines or have special chunking), use the Initiate/UploadPart/Complete flow:

csharp
Copy
Edit
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime.CredentialManagement;
using System.IO;
using System.Collections.Generic;

async Task ManualMultipartUpload(string bucketName, string filePath)
{
    var chain = new CredentialProfileStoreChain();
    chain.TryGetAWSCredentials("aws-csharp", out var creds);
    using var s3 = new AmazonS3Client(creds, Amazon.RegionEndpoint.APSouth1);

    string keyName = Path.GetFileName(filePath);
    var initReq = new InitiateMultipartUploadRequest { BucketName = bucketName, Key = keyName };
    var initResp = await s3.InitiateMultipartUploadAsync(initReq);
    var uploadId = initResp.UploadId;

    var partETags = new List<PartETag>();
    const int partSize = 5 * 1024 * 1024; // 5 MB

    try
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        int partNumber = 1;
        byte[] buffer = new byte[partSize];
        int bytesRead;
        while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            using var ms = new MemoryStream(buffer, 0, bytesRead);
            var uploadPartRequest = new UploadPartRequest
            {
                BucketName = bucketName,
                Key = keyName,
                UploadId = uploadId,
                PartNumber = partNumber,
                InputStream = ms,
                PartSize = bytesRead
            };

            var uploadPartResponse = await s3.UploadPartAsync(uploadPartRequest);
            partETags.Add(new PartETag(partNumber, uploadPartResponse.ETag));
            Console.WriteLine($"Uploaded part {partNumber}, ETag: {uploadPartResponse.ETag}");
            partNumber++;
        }

        var completeRequest = new CompleteMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = keyName,
            UploadId = uploadId,
            PartETags = partETags
        };

        var completeResponse = await s3.CompleteMultipartUploadAsync(completeRequest);
        Console.WriteLine("Multipart upload completed: " + completeResponse.Location);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error during multipart: " + ex.Message);
        await s3.AbortMultipartUploadAsync(new AbortMultipartUploadRequest
        {
            BucketName = bucketName,
            Key = keyName,
            UploadId = uploadId
        });
        Console.WriteLine("Multipart upload aborted.");
    }
}
Notes:

Use PartSize >= 5 MB (except maybe last part).

Keep track of ETag for each part.

Always call Complete or Abort.

1E — Monitoring & cleanup
List in-flight uploads: aws s3api list-multipart-uploads --bucket bucket

List parts: aws s3api list-parts --bucket ... --key ... --upload-id ...

Abort to free parts: aws s3api abort-multipart-upload ...

There can be costs for storing uploaded parts that aren’t completed — use lifecycle to abort old multipart uploads automatically.
2) Pre-signed URLs (temporary access)
What: Pre-signed URLs allow temporary access to an object without giving credentials. Useful for giving clients secure PUT or GET access.

C# — create a pre-signed GET:

csharp
Copy
Edit
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime.CredentialManagement;

var chain = new CredentialProfileStoreChain();
chain.TryGetAWSCredentials("aws-csharp", out var creds);
using var s3 = new AmazonS3Client(creds, Amazon.RegionEndpoint.APSouth1);

var request = new GetPreSignedUrlRequest
{
    BucketName = "your-bucket-name",
    Key = "file.txt",
    Expires = DateTime.UtcNow.AddMinutes(15), // TTL
    Verb = HttpVerb.GET
};

string url = s3.GetPreSignedURL(request);
Console.WriteLine("Pre-signed GET URL: " + url);
Pre-signed PUT (allow upload by client):

csharp
Copy
Edit
var putRequest = new GetPreSignedUrlRequest
{
    BucketName = "your-bucket-name",
    Key = "upload/remote.txt",
    Expires = DateTime.UtcNow.AddMinutes(10),
    Verb = HttpVerb.PUT
};
string putUrl = s3.GetPreSignedURL(putRequest);
Test from terminal:

bash
Copy
Edit
# Download with GET
curl "<pre-signed-get-url>" -o downloaded.txt

# Upload with PUT
curl -X PUT --upload-file localfile.txt "<pre-signed-put-url>"
Security notes:

Keep TTL short.

If object is private, pre-signed URL temporarily grants access.

If bucket has Block Public Access or policies, ensure pre-signed URL operations are allowed