@echo off

del /F /Q /A:H *.suo >nul 2>nul
del /F /Q /S *.pdb >nul 2>nul
rd /S /Q Test\bin >nul 2>nul
rd /S /Q Test\bin >nul 2>nul
rd /S /Q Test\obj >nul 2>nul
rd /S /Q Test\obj >nul 2>nul
rd /S /Q HasK.Controls.Graph.Chart\obj >nul 2>nul
rd /S /Q HasK.Controls.Graph.Chart\obj >nul 2>nul
