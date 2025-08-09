# Day 01 – AWS IAM Setup & CLI Configuration

## Goal
Set up AWS CLI with an IAM user for development, ensuring secure and reusable credentials.

---

## Steps Completed

### 1️⃣ Created IAM User in AWS Console
- **User type:** IAM user  
- **Access type:** Programmatic access (CLI/SDK)  
- **Permissions:** Attached `AdministratorAccess` (for learning phase; will narrow down later)  
- **Tags:** Skipped (optional)  
- **Created** and **downloaded CSV** containing:
  - **Access Key ID**
  - **Secret Access Key**

---

### 2️⃣ Installed & Configured AWS CLI
- Verified AWS CLI installation:
```bash
aws --version
Configured profile for C# AWS learning:

bash
Copy
Edit
aws configure --profile aws-csharp
Access Key ID → from CSV

Secret Access Key → from CSV

Region → ap-south-1 (Mumbai)

Output format → json

3️⃣ Verified Credentials
Ran:

bash
Copy
Edit
aws sts get-caller-identity --profile aws-csharp
Output confirmed:

json
Copy
Edit
{
    "UserId": "AIDAEXAMPLEUSERID",
    "Account": "123456789012",
    "Arn": "arn:aws:iam::123456789012:user/dev-yourname"
}
4️⃣ Security Best Practices
Never commit real credentials to GitHub.

Created credentials.example for safe sharing:

ini
Copy
Edit
[aws-csharp]
aws_access_key_id = YOUR_ACCESS_KEY_ID
aws_secret_access_key = YOUR_SECRET_ACCESS_KEY
region = ap-south-1
Added .gitignore entries:

Copy
Edit
.aws/
*.csv
Stored real keys in password manager.

Outcome
AWS CLI working with aws-csharp profile

Environment ready for AWS SDK integration in C#

Safe credential management practices in place