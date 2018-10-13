@echo off
cls
type b-art.txt

packages\FAKE\tools\FAKE.exe build-net4.fsx %*
