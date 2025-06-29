#!/bin/bash
set -e

TB_DIR="$HOME/tigerbeetle"
TB_BIN="$TB_DIR/tigerbeetle"
TB_DATA="$TB_DIR/0_0.tigerbeetle"
SERVICE_PATH="/etc/systemd/system/tigerbeetle.service"
TB_PORT=4000

ACCOUNT_ID=1000
LEDGER_ID=1
ACCOUNT_CODE=1000

mkdir -p "$TB_DIR"
cd "$TB_DIR"

echo "Downloading TigerBeetle v$VERSION..."
curl -Lo tigerbeetle.zip https://linux.tigerbeetle.com && unzip -o tigerbeetle.zip
./tigerbeetle version

echo "Formatting file..."
rm -f ./0_0.tigerbeetle
./tigerbeetle format --cluster=0 --replica=0 --replica-count=1 ./0_0.tigerbeetle

echo "Checking if the service already exists..."

if systemctl status tigerbeetle &> /dev/null; then
    echo "Service exists... restarting existing tigerbeetle service"
    sudo systemctl restart tigerbeetle
else
    echo "Service does not exist... Creating systemd service"
    sudo tee "$SERVICE_PATH" > /dev/null <<EOF
    [Unit]
    Description=TigerBeetle Ledger Server
    After=network.target

    [Service]
    Type=simple
    WorkingDirectory=$TB_DIR
    ExecStart=$TB_DIR/tigerbeetle start --addresses=$TB_PORT ./0_0.tigerbeetle
    Restart=on-failure
    User=$USER_NAME

    [Install]
    WantedBy=multi-user.target
EOF
    sudo systemctl daemon-reload
    sudo systemctl enable tigerbeetle
    sudo systemctl start tigerbeetle

    echo "Successfully started tigerbeetle..."

fi







