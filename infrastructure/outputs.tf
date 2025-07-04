output "private_key_pem" {
  value     = tls_private_key.ssh_key.private_key_pem
  sensitive = true
}

output "lambda_function_url" {
  description = "Lambda Function URL"
  value       = aws_lambda_function_url.ca_signer_url.function_url
}
