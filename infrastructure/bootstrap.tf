provider "aws" {
  region = "af-south-1"
}

resource "aws_s3_bucket" "terraform_state" {
  bucket = "retail-bank-tfstate"
}

resource "aws_dynamodb_table" "terraform_locks" {
  name         = "terraform_locks"
  billing_mode = "PAY_PER_REQUEST"
  attribute {
    name = "LockID"
    type = "S"
  }
  hash_key = "LockID"
}
