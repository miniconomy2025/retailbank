variable "ca_private_key" {
  description = "CA private key for certificate signing"
  type        = string
  sensitive   = true
  default     = "PLACEHOLDER_PRIVATE_KEY"
}

variable "ca_certificate" {
  description = "CA certificate for certificate signing"
  type        = string
  sensitive   = true
  default     = "PLACEHOLDER_CERTIFICATE"
}
