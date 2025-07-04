resource "aws_secretsmanager_secret" "ca_private_key" {
  name        = "ca-signer/private-key"
  description = "CA private key for certificate signing"

  tags = {
    Name = "ca-private-key"
  }
}

resource "aws_secretsmanager_secret_version" "ca_private_key" {
  secret_id     = aws_secretsmanager_secret.ca_private_key.id
  secret_string = var.ca_private_key
}

resource "aws_secretsmanager_secret" "ca_certificate" {
  name        = "ca-signer/certificate"
  description = "CA certificate for certificate signing"

  tags = {
    Name = "ca-certificate"
  }
}

resource "aws_secretsmanager_secret_version" "ca_certificate" {
  secret_id     = aws_secretsmanager_secret.ca_certificate.id
  secret_string = var.ca_certificate
}

resource "aws_iam_role_policy" "ca_lambda_secrets_policy" {
  name = "ca-lambda-secrets-policy"
  role = aws_iam_role.ca_lambda_exec.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "secretsmanager:GetSecretValue"
        ]
        Resource = [
          aws_secretsmanager_secret.ca_private_key.arn,
          aws_secretsmanager_secret.ca_certificate.arn
        ]
      }
    ]
  })
}
