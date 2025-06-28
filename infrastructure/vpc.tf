resource "aws_vpc" "retail_bank_vpc" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = {
    Name = "retail-bank-vpc"
  }
}

resource "aws_internet_gateway" "retail_bank_igw" {
  vpc_id = aws_vpc.retail_bank_vpc.id

  tags = {
    Name = "retail-bank-igw"
  }
}

resource "aws_subnet" "retail_bank_public_subnet" {
  vpc_id                  = aws_vpc.retail_bank_vpc.id
  cidr_block              = "10.0.1.0/24"
  map_public_ip_on_launch = true
  availability_zone       = "af-south-1a"

  tags = {
    Name = "retail-bank-public-subnet"
  }
}

resource "aws_route_table" "retail_bank_public_route_table" {
  vpc_id = aws_vpc.retail_bank_vpc.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.retail_bank_igw.id
  }

  tags = {
    Name = "retail-bank-public-route-table"
  }
}

resource "aws_route_table_association" "retail_bank_public_route_table_association" {
  subnet_id      = aws_subnet.retail_bank_public_subnet.id
  route_table_id = aws_route_table.retail_bank_public_route_table.id
}
