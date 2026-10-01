# Insidden FTP Connector v1.0.0

A lightweight, standalone GUI utility designed to seamlessly authenticate and connect to FTP servers via native OS file managers (Windows File Explorer and macOS Finder). 

This application resolves common issues with built-in FTP authentication by providing a reliable login interface, securely managing passwords in native credential managers, and safely passing credentials to the system.

<img width="1897" height="355" alt="LOGO_2024_SMALL" src="https://github.com/user-attachments/assets/cd51341c-aeb3-4765-a1de-3c64fba1e983" />

## Key Features
- **Seamless Integration:** Opens the target FTP server directly in Windows Explorer or macOS Finder.
- **Secure Credential Storage:** Uses native OS APIs (Windows Credential Manager and macOS Keychain) to securely save, read, and clear passwords.
- **Standalone & Lightweight:** Native executables compiled without third-party dependencies.

---

## 🪟 Windows Version

The Windows version is built using C# (WinForms) and runs as a silent native GUI application, preventing background console (`cmd.exe`) windows.

### Compilation
Compile the application using the native .NET Framework compiler included in Windows:
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /win32icon:icon.ico /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /out:InsiddenFTP.exe Program.cs
```

### Digital Signature
To sign the executable with a Certum hardware token (preventing SmartScreen warnings):
```cmd
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe" sign /n "TIMEWISE LLC" /fd sha256 /tr [http://time.certum.pl](http://time.certum.pl) /td sha256 InsiddenFTP.exe
```

### Usage
Run `InsiddenFTP.exe`, enter your login details, and click **Connect**. You can choose to save your credentials for instant access on future launches, or clear them at any time directly from the app.

---

## 🍏 macOS Version

The macOS version is built using Swift and SwiftUI, leveraging the macOS Keychain for secure storage. 
*Note: Due to macOS system limitations, Finder mounts FTP servers in **Read-Only** mode.*

### Compilation
You do not need Xcode to compile the application. Use the built-in Swift compiler via Terminal. Run the following commands in the directory containing `InsiddenFTP.swift`:

**1. Create the application bundle structure:**
```bash
mkdir -p InsiddenFTP.app/Contents/MacOS
```

**2. Compile the source code:**
```bash
swiftc InsiddenFTP.swift -parse-as-library -o InsiddenFTP.app/Contents/MacOS/InsiddenFTP
```

**3. Generate the required `Info.plist` file:**
```bash
cat << 'EOF' > InsiddenFTP.app/Contents/Info.plist
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "[http://www.apple.com/DTDs/PropertyList-1.0.dtd](http://www.apple.com/DTDs/PropertyList-1.0.dtd)">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>InsiddenFTP</string>
    <key>CFBundleIdentifier</key>
    <string>com.timewise.insiddenftp</string>
    <key>CFBundleName</key>
    <string>InsiddenFTP</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>LSMinimumSystemVersion</key>
    <string>11.0</string>
    <key>LSUIElement</key>
    <false/>
</dict>
</plist>
EOF
```

### Bypassing Gatekeeper (For downloaded releases)
If you download the compiled `.app` from a release archive, macOS Gatekeeper may block it. To remove the quarantine attribute, run this command in Terminal before launching:
```bash
xattr -cr /path/to/InsiddenFTP.app
```
