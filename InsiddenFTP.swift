import SwiftUI
import Security
import AppKit

// MARK: - Keychain Helper
/// A helper class to securely store, read, and delete credentials in macOS Keychain.
class KeychainHelper {
    static let serverName = "kt.insidden.com"
    
    static func save(username: String, password: String) {
        delete() // Remove existing entry before saving a new one
        
        guard let passwordData = password.data(using: .utf8) else { return }
        
        let query: [String: Any] = [
            kSecClass as String: kSecClassInternetPassword,
            kSecAttrServer as String: serverName,
            kSecAttrAccount as String: username,
            kSecValueData as String: passwordData
        ]
        
        SecItemAdd(query as CFDictionary, nil)
    }
    
    static func read() -> (username: String, password: String)? {
        let query: [String: Any] = [
            kSecClass as String: kSecClassInternetPassword,
            kSecAttrServer as String: serverName,
            kSecReturnAttributes as String: true,
            kSecReturnData as String: true,
            kSecMatchLimit as String: kSecMatchLimitOne
        ]
        
        var item: CFTypeRef?
        let status = SecItemCopyMatching(query as CFDictionary, &item)
        
        if status == errSecSuccess,
           let dict = item as? [String: Any],
           let username = dict[kSecAttrAccount as String] as? String,
           let passwordData = dict[kSecValueData as String] as? Data,
           let password = String(data: passwordData, encoding: .utf8) {
            return (username, password)
        }
        
        return nil
    }
    
    static func delete() {
        let query: [String: Any] = [
            kSecClass as String: kSecClassInternetPassword,
            kSecAttrServer as String: serverName
        ]
        SecItemDelete(query as CFDictionary)
    }
}

// MARK: - User Interface
struct ContentView: View {
    @State private var username = ""
    @State private var password = ""
    @State private var rememberMe = false
    @State private var hasSavedCredentials = false
    
    let ftpServer = "kt.insidden.com"
    
    var body: some View {
        VStack(spacing: 15) {
            HStack {
                Text("Username:")
                    .frame(width: 80, alignment: .trailing)
                TextField("", text: $username)
                    .textFieldStyle(RoundedBorderTextFieldStyle())
            }
            
            HStack {
                Text("Password:")
                    .frame(width: 80, alignment: .trailing)
                SecureField("", text: $password)
                    .textFieldStyle(RoundedBorderTextFieldStyle())
            }
            
            HStack {
                Spacer().frame(width: 88)
                Toggle("Save credentials", isOn: $rememberMe)
                Spacer()
            }
            
            HStack(spacing: 15) {
                Button("Connect") {
                    connectToFtp()
                }
                .keyboardShortcut(.defaultAction) // Binds to Enter key
                
                Button("Cancel") {
                    NSApplication.shared.terminate(nil)
                }
                .keyboardShortcut(.cancelAction) // Binds to Esc key
            }
            .padding(.top, 5)
            
            Button("Clear Saved Data") {
                clearData()
            }
            .frame(width: 150)
        }
        .padding(30)
        .onAppear {
            loadSavedData()
        }
    }
    
    private func loadSavedData() {
        if let saved = KeychainHelper.read() {
            self.username = saved.username
            self.password = saved.password
            self.rememberMe = true
            self.hasSavedCredentials = true
        }
    }
    
    private func clearData() {
        KeychainHelper.delete()
        self.username = ""
        self.password = ""
        self.rememberMe = false
        self.hasSavedCredentials = false
        
        let alert = NSAlert()
        alert.messageText = "Information"
        alert.informativeText = "Saved credentials have been removed from the system."
        alert.alertStyle = .informational
        alert.addButton(withTitle: "OK")
        alert.runModal()
    }
    
    private func connectToFtp() {
        guard !username.isEmpty else { return }
        
        if rememberMe {
            KeychainHelper.save(username: username, password: password)
        } else if hasSavedCredentials {
            KeychainHelper.delete()
        }
        
        // URL-encode credentials to safely handle special characters (e.g. @, <)
        let encodedUser = username.addingPercentEncoding(withAllowedCharacters: .urlUserAllowed) ?? username
        let encodedPass = password.addingPercentEncoding(withAllowedCharacters: .urlPasswordAllowed) ?? password
        
        let urlString = "ftp://\(encodedUser):\(encodedPass)@\(ftpServer)/"
        
        if let url = URL(string: urlString) {
            // Open the URL in the default application (Finder for ftp://)
            NSWorkspace.shared.open(url)
        }
        
        // Terminate the application after passing the URL to Finder
        NSApplication.shared.terminate(nil)
    }
}

// MARK: - Application Entry Point
@main
struct FtpApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
                .frame(width: 340, height: 230)
                .fixedSize() // Prevents window resizing
        }
        .windowResizability(.contentSize)
        .windowStyle(HiddenTitleBarWindowStyle())
        .commands {
            // Remove standard menu items to make it look like a standalone dialog
            CommandGroup(replacing: .newItem) { }
        }
    }
}