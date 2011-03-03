REM make sure the path does not have .PROTO in it (it never should, that'd be ridiuclous)
REM @ECHO OFF
SET FNAME=%1
SET FNAME=%FNAME:.proto=.cs%
SET NEWDIR=..\..\Framework\Protos\%FNAME:.cs=%
echo %NEWDIR%

..\..\Binaries\ProtoBuff\protogen.exe -i:%1 -o:%FNAME%

..\..\Binaries\ProtoBuff\protoc.exe --cpp_out=. %1

mkdir %NEWDIR%
move %FNAME% %NEWDIR%
move %FNAME:.cs=.pb.cc% %NEWDIR%
move %FNAME:.cs=.pb.h% %NEWDIR%
copy %FNAME:.cs=.proto% %NEWDIR%

@ECHO ON