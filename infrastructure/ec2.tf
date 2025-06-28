resource "aws_instance" "retail_bank_ec2" {
  ami                    = "ami-0cf00a97588a6d5c9"
  instance_type          = "t3.micro"
  subnet_id              = aws_subnet.retail_bank_public_subnet.id
  vpc_security_group_ids = [aws_security_group.retail_bank_ec2_sg.id]
  key_name               = aws_key_pair.ssh_key_pair.key_name
  tags = {
    Name = "retail_bank_ec2"
  }
}

resource "aws_security_group" "retail_bank_ec2_sg" {
  name        = "retail_bank_ec2_sg"
  description = "Allow access to the retail_bank_ec2 instance"
  vpc_id      = aws_vpc.retail_bank_vpc.id

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "tls_private_key" "ssh_key" {
  algorithm = "RSA"
  rsa_bits  = 4096
}

resource "aws_key_pair" "ssh_key_pair" {
  key_name   = "retail-bank-ssh-key"
  public_key = tls_private_key.ssh_key.public_key_openssh
}
