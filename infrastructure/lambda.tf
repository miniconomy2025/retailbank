resource "aws_iam_role" "ca_lambda_exec" {
  name = "ca-lambda-exec-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "ca_lambda_basic_execution" {
  role       = aws_iam_role.ca_lambda_exec.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

data "archive_file" "ca_function_zip" {
  type        = "zip"
  source_dir  = "${path.module}/../ca-function"
  output_path = "${path.module}/ca-function.zip"
}

resource "aws_lambda_function" "ca_signer" {
  function_name = "ca-signer"
  handler       = "index.handler"
  runtime       = "nodejs18.x"
  role          = aws_iam_role.ca_lambda_exec.arn
  timeout       = 30

  filename         = data.archive_file.ca_function_zip.output_path
  source_code_hash = data.archive_file.ca_function_zip.output_base64sha256

  environment {
    variables = {
      CA_PRIVATE_KEY_SECRET_ARN = aws_secretsmanager_secret.ca_private_key.arn
      CA_CERTIFICATE_SECRET_ARN = aws_secretsmanager_secret.ca_certificate.arn
    }
  }

  tags = {
    Name = "ca-signer"
  }
}

resource "aws_lambda_function_url" "ca_signer_url" {
  function_name      = aws_lambda_function.ca_signer.function_name
  authorization_type = "NONE"

  cors {
    allow_credentials = false
    allow_headers     = ["date", "keep-alive"]
    allow_methods     = ["*"]
    allow_origins     = ["*"]
    expose_headers    = ["date", "keep-alive"]
    max_age           = 86400
  }
}
