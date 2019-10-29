#!/bin/sh
MagicOnionCodeGenerator/win-x64/moc -i ../Server/Server.csproj -o ../../Client/Assets/Generated/MagicOnion.Generated.cs

MessagePackUniversalCodeGenerator/win-x64/mpc -i ../Server/Server.csproj -o ../../Client/Assets/Generated/MessagePack.Generated.cs

rm -f ../Server/__buildtemp*
