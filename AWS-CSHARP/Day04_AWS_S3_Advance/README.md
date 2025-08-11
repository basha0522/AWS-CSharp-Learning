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

3) Versioning
What: Keep multiple versions of an object. Each PUT creates a new version (unless same version id). Useful to recover deleted/overwritten objects.

Enable versioning (CLI):

bash
Copy
Edit
aws s3api put-bucket-versioning \
  --bucket your-bucket-name \
  --versioning-configuration Status=Enabled \
  --profile aws-csharp
Check status:

bash
Copy
Edit
aws s3api get-bucket-versioning --bucket your-bucket-name --profile aws-csharp
List versions (CLI):

bash
Copy
Edit
aws s3api list-object-versions --bucket your-bucket-name --profile aws-csharp
C# enable versioning:

csharp
Copy
Edit
using Amazon.S3.Model;

var req = new PutBucketVersioningRequest
{
    BucketName = "your-bucket-name",
    VersioningConfig = new S3BucketVersioningConfig { Status = VersionStatus.Enabled }
};
await s3Client.PutBucketVersioningAsync(req);
Restore older version: download by --version-id or use SDK GetObjectRequest with VersionId.

Note: Deleting an object with versioning enabled creates a delete marker; older versions still exist.

4) Lifecycle policies
Use lifecycle rules to transition objects to cheaper storage (Standard-IA, Glacier, Deep Archive) or expire/delete them, or to abort incomplete multipart uploads.

Example lifecycle JSON (transition to STANDARD_IA after 30d, expire after 365d):
lifecycle.json

json
Copy
Edit
{
  "Rules": [
    {
      "ID": "MoveToIAAndExpire",
      "Filter": { "Prefix": "" },
      "Status": "Enabled",
      "Transitions": [
        {
          "Days": 30,
          "StorageClass": "STANDARD_IA"
        }
      ],
      "Expiration": {
        "Days": 365
      }
    },
    {
      "ID": "AbortMultipartUploads",
      "Filter": { "Prefix": "" },
      "Status": "Enabled",
      "AbortIncompleteMultipartUpload": { "DaysAfterInitiation": 7 }
    }
  ]
}
Apply with CLI:

bash
Copy
Edit
aws s3api put-bucket-lifecycle-configuration --bucket your-bucket-name --lifecycle-configuration file://lifecycle.json --profile aws-csharp
Notes:

Choose storage class carefully: retrieval fees and latency differ (GLACIER vs GLACIER_INSTANT_RETRIEVAL etc.).

Lifecycle rules are powerful for cost control.

5) Server-side encryption (SSE)
Three options:

SSE-S3 (Amazon S3 managed keys) — easiest: AWS manages encryption keys.

SSE-KMS (AWS KMS keys) — you control KMS key (recommended for sensitive data and audit).

SSE-C (customer-provided keys) — not common for typical apps.

Enable default encryption on bucket (CLI) — SSE-S3 example:
encryption-s3.json

json
Copy
Edit
{
  "Rules": [
    {
      "ApplyServerSideEncryptionByDefault": {
        "SSEAlgorithm": "AES256"
      }
    }
  ]
}
bash
Copy
Edit
aws s3api put-bucket-encryption \
  --bucket your-bucket-name \
  --server-side-encryption-configuration file://encryption-s3.json \
  --profile aws-csharp
SSE-KMS (default to KMS key):
json
Copy
Edit
{
  "Rules": [
    {
      "ApplyServerSideEncryptionByDefault": {
        "SSEAlgorithm": "aws:kms",
        "KMSMasterKeyID": "arn:aws:kms:region:account-id:key/key-id"
      }
    }
  ]
}
C# per-object encryption with SSE-S3:

csharp
Copy
Edit
var putReq = new PutObjectRequest
{
    BucketName = bucketName,
    Key = "file.txt",
    FilePath = "localfile.txt",
    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256 // SSE-S3
};
await s3.PutObjectAsync(putReq);
C# per-object SSE-KMS:

csharp
Copy
Edit
var putReq = new PutObjectRequest
{
    BucketName = bucketName,
    Key = "file.txt",
    FilePath = "localfile.txt",
    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AWSKMS,
    ServerSideEncryptionKeyManagementServiceKeyId = "arn:aws:kms:..."
};
await s3.PutObjectAsync(putReq);
Permissions for KMS: ensure IAM role/user has kms:Encrypt, kms:Decrypt, kms:GenerateDataKey*, and that the KMS key policy allows the principal and S3 to use it.

6) IAM permissions needed (minimum)
For these features you’ll need S3 actions (example list):

s3:PutObject, s3:GetObject, s3:DeleteObject

s3:ListBucket, s3:ListBucketMultipartUploads

s3:CreateMultipartUpload, s3:UploadPart, s3:CompleteMultipartUpload, s3:AbortMultipartUpload

s3:PutObjectAcl (if setting ACL)

s3:PutBucketVersioning, s3:GetBucketVersioning

s3:PutBucketLifecycle, s3:GetBucketLifecycle

s3:PutEncryptionConfiguration or s3:PutBucketEncryption (depends)

For SSE-KMS: kms:Encrypt, kms:Decrypt, kms:GenerateDataKey, kms:DescribeKey as applicable

Tip: Use least privilege and test with a dedicated developer IAM user or role.

7) Tests & verification (quick checklist)
aws sts get-caller-identity --profile aws-csharp (confirm identity)

Create a test bucket (or use existing)

Multipart: run aws s3 cp bigfile.zip ... and aws s3api list-multipart-uploads to watch progress

Pre-signed URL: generate, use curl to GET/PUT

Versioning: enable, upload same object multiple times, aws s3api list-object-versions

Lifecycle: apply JSON, check in console or wait for transition (can simulate with TestStorageClass API? Usually wait)

SSE: upload with ServerSideEncryptionMethod, then check object metadata in console — it shows “SSE-S3” or “SSE-KMS”