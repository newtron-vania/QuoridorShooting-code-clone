using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;

public static class DataDecoder
{
    public static byte[] DecryptQED(byte[] blob, string keyString)
    {
        byte[] key = FromHex(keyString);
        using var ms = new MemoryStream(blob);
        using var br = new BinaryReader(ms);

        // Header
        if (br.ReadByte() != 'Q' || br.ReadByte() != 'E' || br.ReadByte() != 'D' || br.ReadByte() != 'J')
            throw new InvalidDataException("Bad magic");
        byte ver = br.ReadByte();
        if (ver != 1) throw new InvalidDataException($"Unsupported ver {ver}");
        byte alg = br.ReadByte();
        if (alg != 1) throw new InvalidDataException("Unsupported alg");
        byte[] nonce = br.ReadBytes(12);

        // Ciphertext || Tag(16)
        int rem = (int)(ms.Length - ms.Position);
        if (rem < 16) throw new InvalidDataException("Truncated");
        byte[] ctOnly = new byte[rem - 16];
        byte[] tag = new byte[16];
        br.Read(ctOnly, 0, ctOnly.Length);
        br.Read(tag, 0, 16);

        byte[] pt = new byte[ctOnly.Length];
        try
        {
            pt = DecryptWithAesGcm(nonce, ctOnly, tag, key);
        }
        catch (PlatformNotSupportedException)
        {
            pt = DecryptWithBC(nonce, ctOnly, tag, key);
        }
        catch (NotImplementedException)
        {
            pt = DecryptWithBC(nonce, ctOnly, tag, key);
        }
        // raw DEFLATE inflate
        using var comp = new MemoryStream(pt);
        using var deflate = new DeflateStream(comp, CompressionMode.Decompress);
        using var outMemoryStream = new MemoryStream();
        deflate.CopyTo(outMemoryStream);
        return outMemoryStream.ToArray();
    }
    private static byte[] DecryptWithAesGcm(byte[] nonce, byte[] ciphertext, byte[] tag, byte[] key)
    {
        byte[] plaintext = new byte[ciphertext.Length];
        using var gcm = new AesGcm(key);
        gcm.Decrypt(nonce, ciphertext, tag, plaintext, ReadOnlySpan<byte>.Empty); // no AAD
        return plaintext;
    }
    private static byte[] DecryptWithBC(byte[] nonce, byte[] ciphertext, byte[] tag, byte[] key)
    {
        var combined = new byte[ciphertext.Length + tag.Length];
        Buffer.BlockCopy(ciphertext, 0, combined, 0, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, combined, ciphertext.Length, tag.Length);

        var gcm = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(new KeyParameter(key), 128, nonce, null);
        gcm.Init(false, parameters);

        var outBuffer = new byte[gcm.GetOutputSize(combined.Length)];
        int len = gcm.ProcessBytes(combined, 0, combined.Length, outBuffer, 0);
        len += gcm.DoFinal(outBuffer, len);

        var plaintext = new byte[len];
        Buffer.BlockCopy(outBuffer, 0, plaintext, 0, len);
        return plaintext;
    }
    private static byte[] FromHex(string hex)
    {
        hex = (hex ?? "").Trim();
        // 공백/하이픈/0x 제거
        hex = hex.Replace(" ", "").Replace("-", "");
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) hex = hex.Substring(2);
        if (hex.Length % 2 != 0) throw new ArgumentException("hex length must be even");
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return bytes;
    }
}
