// Retreive path of the file via user input
// Retreive the crypt key via user input
// Place an encrypted file in a sub directory as the original file named crypt folder
// Write out the decrypted text to the console
// Finished
// Remember comments to explain what is done.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

Console.WriteLine("Enter the absolute path of the file you want to encrypt: ");
string filePath = Console.ReadLine();
Console.WriteLine("Enter the crypt key (32-byte hex key, 64 characters): ");
string cryptKey = Console.ReadLine();

// Trim whitespace and newlines from the input and pad if needed
cryptKey = cryptKey?.Trim();


Console.WriteLine("Key Length: " + cryptKey.Length);
if (cryptKey.Length != 64)
{
    Console.WriteLine("The key was not 64 characters long, zeros were added as padding to the right");
}

// Ensure the key is 64 characters long
if (cryptKey.Length < 64)
{
    cryptKey = cryptKey.PadRight(64, '0');  // Pad with zeros
}

// Convert hex string to byte array
byte[] keyBytes = new byte[32];  // 256-bit key = 32 bytes
for (int i = 0; i < cryptKey.Length; i += 2)
{
    keyBytes[i / 2] = Convert.ToByte(cryptKey.Substring(i, 2), 16);
}

var aes = Aes.Create();
// Generate an initialization vector (IV)
aes.GenerateIV();
byte[] iv = aes.IV;
aes.Key = keyBytes;  // Use the byte array as the AES key
aes.Padding = PaddingMode.PKCS7;  // Ensure the plaintext fits into blocks
aes.Mode = CipherMode.CBC;  // CBC mode

// Create the "crypt" folder if it doesn't exist
string cryptFolderPath = Path.Combine(Path.GetDirectoryName(filePath), "crypt");
if (!Directory.Exists(cryptFolderPath))
{
    Directory.CreateDirectory(cryptFolderPath);
}

// Create the encrypted file path
string encryptedFilePath = Path.Combine(cryptFolderPath, Path.GetFileName(filePath) + ".enc");

// Encrypt the file
using (FileStream inputFile = new FileStream(filePath, FileMode.Open, FileAccess.Read))
using (FileStream outputFile = new FileStream(encryptedFilePath, FileMode.Create, FileAccess.Write))
using (CryptoStream cryptoStream = new CryptoStream(outputFile, aes.CreateEncryptor(), CryptoStreamMode.Write))
{
    // Write the IV at the beginning of the encrypted file
    outputFile.Write(iv, 0, iv.Length);

    // Process the input file and encrypt it
    byte[] buffer = new byte[1024];
    int bytesRead;
    while ((bytesRead = inputFile.Read(buffer, 0, buffer.Length)) > 0)
    {
        cryptoStream.Write(buffer, 0, bytesRead);
    }
    cryptoStream.FlushFinalBlock();
}

Console.WriteLine("File encrypted and saved at: " + encryptedFilePath);

// Decrypt the file
using (FileStream encryptedFile = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read))
{
    // Read the IV from the encrypted file
    byte[] ivFromFile = new byte[16];  // AES block size is 16 bytes
    encryptedFile.Read(ivFromFile, 0, ivFromFile.Length);

    // Set the IV from the file
    aes.IV = ivFromFile;

    using (CryptoStream cryptoStream = new CryptoStream(encryptedFile, aes.CreateDecryptor(), CryptoStreamMode.Read))
    using (StreamReader reader = new StreamReader(cryptoStream))
    {
        // Read and output the decrypted text
        string decryptedText = reader.ReadToEnd();
        Console.WriteLine("Decrypted content:");
        Console.WriteLine(decryptedText);
    }
}



/*
 * // Gets the filepath
// Gets the symmetric crypt key
// it is hexadecimal
checks the length, adds necessary padding
splits the string into 32 bytes, two characters is one byte. For example ef is a byte
Checks the length

defines aes
Sets the initialization vector
sets the crypt key
sets padding and mode

Checks if the crypt folder exists, if not makes one
Creates the encrypted file path inside the crypt folder by combining the crypt folder and the file name + .enc

Opens a file stream with read access to the input file which is the non encrypted file
Opens a file stream with write access to the output file which is the encrypted file
Opens a Cryptostream that has knowledge of the two files aboce, and it is used to encrypt and it encrypts with the encrypted file path
It writes the iv at the beginning
creates a buffer with 1024 bytes
creates a variable named bytes read
Reads a portion of the input file at the time, and sets bytesRead based on that using the Read method
Writes the buffer based on how many bytes is read into the outputfile using the cryptoStream.Write method
flushes the final block

Opens a new filestream with access to the encrypted file path
Each aes block is 16 bytes, so it defines a byte array that the defines the ivFromFile
And it reads the first 16 bytes of the file using the Read method and referencing ivFromFile
sets IV
Creates a new crypto stream that uses the encrypted file, creates a decryptor, reads from it
Creates a StreamReader that reads from the crypto stream
defines the decrypted content using reader.ReadToEnd();
 */