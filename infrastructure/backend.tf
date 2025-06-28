# terraform {
#   backend "s3" {
#     bucket         = "retail-bank-terraform-state"
#     key            = "terraform.tfstate"
#     region         = "af-south-1"
#     dynamodb_table = "terraform_locks"
#     encrypt        = true
#   }
# }
