Day 02 – AWS S3 Bucket Creation (CLI Notes)
Step 1 – Create a folder for Day 02 work
In your cloned GitHub repo:

Copy
Edit
Day02_AWS_S3/
Inside create:

Day02_Notes.md → For today’s learning notes.

demo/ → C# project will go here later.

Step 2 – Pick a unique bucket name
Rules:

Lowercase only.

No spaces or underscores.

Must be globally unique in all AWS.

Example bucket name:

Copy
Edit
abdul-aws-s3-demo-20250809
Step 3 – Create the bucket
Windows Command Prompt (cmd.exe)

c
Copy
Edit
aws s3api create-bucket --bucket abdul-aws-s3-demo-20250809 --region ap-south-1 --create-bucket-configuration LocationConstraint=ap-south-1 --profile aws-csharp
PowerShell

powershell
Copy
Edit
aws s3api create-bucket `
    --bucket abdul-aws-s3-demo-20250809 `
    --region ap-south-1 `
    --create-bucket-configuration LocationConstraint=ap-south-1 `
    --profile aws-csharp
Step 4 – Verify bucket creation
Command Prompt (cmd.exe)

cmd
Copy
Edit
aws s3 ls --profile aws-csharp
PowerShell

powershell
Copy
Edit
aws s3 ls --profile aws-csharp
Expected output:

yaml
Copy
Edit
2025-08-09  12:15:32  abdul-aws-s3-demo-20250809
Step 5 – Upload a test file
Command Prompt (cmd.exe)

cmd
Copy
Edit
echo Hello AWS S3 > testfile.txt
aws s3 cp testfile.txt s3://abdul-aws-s3-demo-20250809/ --profile aws-csharp
PowerShell

powershell
Copy
Edit
"Hello AWS S3" | Out-File -Encoding utf8 testfile.txt
aws s3 cp testfile.txt s3://abdul-aws-s3-demo-20250809/ --profile aws-csharp
Step 6 – List objects in bucket
Command Prompt (cmd.exe)

cmd
Copy
Edit
aws s3 ls s3://abdul-aws-s3-demo-20250809/ --profile aws-csharp
PowerShell

powershell
Copy
Edit
aws s3 ls s3://abdul-aws-s3-demo-20250809/ --profile aws-csharp
Expected output (if test file uploaded):

yaml
Copy
Edit
2025-08-09  12:20:01        13 testfile.txt