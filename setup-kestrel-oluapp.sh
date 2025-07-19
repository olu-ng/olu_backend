#!/bin/bash

echo "🔧 Creating kestrel-oluapp.service..."

sudo tee /etc/systemd/system/kestrel-oluapp.service > /dev/null << 'EOF'
[Unit]
Description=Olu .NET Backend App
After=network.target

[Service]
WorkingDirectory=/opt/olu_backend/app
ExecStart=/usr/bin/dotnet /opt/olu_backend/app/OluBackendApp.dll
Restart=always
RestartSec=10
SyslogIdentifier=oluapp
User=ubuntu
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOF

echo "🔁 Reloading systemd daemon..."
sudo systemctl daemon-reload

echo "📌 Enabling kestrel-oluapp to start on boot..."
sudo systemctl enable kestrel-oluapp

echo "🚀 Starting kestrel-oluapp..."
sudo systemctl start kestrel-oluapp

echo "✅ kestrel-oluapp service is now active"
