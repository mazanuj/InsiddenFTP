# Insidden FTP Connector

<img width="1897" height="355" alt="LOGO_2024_SMALL" src="https://github.com/user-attachments/assets/cd51341c-aeb3-4765-a1de-3c64fba1e983" />

## Overview
Insidden FTP Connector is a lightweight, standalone Windows GUI utility written in C# (WinForms). It is specifically designed to provide seamless, secure access to the `kt.insidden.com` FTP server directly through Windows File Explorer. 

Since modern web browsers no longer support the `ftp://` protocol, this utility bridges the gap by providing a native authentication window, managing credentials securely, and automatically mounting the FTP directory in the Windows shell.

## Key Features
* **Bypasses Browser Restrictions:** Opens the FTP server directly in Windows File Explorer instead of relying on deprecated browser support.
* **Credential Management:** Integrates natively with the Windows Credential Manager (`advapi32.dll` and `cmdkey.exe`). It securely reads, saves, and deletes your login information.
* **Reliable Connection:** Passes credentials directly via the URL to File Explorer, bypassing internal Windows caching bugs and ensuring a 100% success rate on login.
* **Standalone Executable:** Compiled as a hidden-console Windows executable (`winexe`). No installation is required.
* **Data Management:** Includes a built-in "Clear Saved Data" button to instantly purge your stored FTP credentials from the system.

## How to Build (Compilation)
You do not need Visual Studio to build this application. It can be compiled using the built-in C# compiler (`csc.exe`) included with the .NET Framework on Windows.

Open the Command Prompt (`cmd`) in the directory containing `Program.cs` and `icon.ico`, then run the following command:

```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /win32icon:icon.ico /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /out:InsiddenFTP.exe Program.cs
```

## How to Sign (Digital Signature)
To prevent Windows Defender warnings and ensure the integrity of the executable, the compiled file must be signed using a hardware token (e.g., Certum) via the Windows SDK `signtool.exe`.

Run the following command in the Command Prompt to sign `InsiddenFTP.exe`:

```cmd
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe" sign /n "TIMEWISE LLC" /fd sha256 /tr http://time.certum.pl /td sha256 InsiddenFTP.exe
```

*Note: You will be prompted by your smart card / hardware token software to enter your PIN during the signing process.*
