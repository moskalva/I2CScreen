
## Info
dependency:
- libgdiplus


Device IC: SS1315

sudo i2cdetect -y 1
https://www.raspberrypi-spy.co.uk/2018/04/i2c-oled-display-module-with-raspberry-pi/

cd ./Projects/TryI2CMonitor
dotnet build ./My.I2C.Screen/My.I2C.Screen.csproj 
sudo ./My.I2C.Screen/bin/Debug/net6.0/My.I2C.Screen 1