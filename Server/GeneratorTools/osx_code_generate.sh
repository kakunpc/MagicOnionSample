#!/bin/sh
MagicOnionCodeGenerator/osx-x64/moc -i ../Server/Server.csproj -o ../../Client/Assets/Generated/MagicOnion.Generated.cs

MessagePackUniversalCodeGenerator/osx-x64/mpc -i ../Server/Server.csproj -o ../../Client/Assets/Generated/MessagePack.Generated.cs

rm -f ../Server/__buildtemp*
