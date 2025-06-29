using System.IO;
using System.IO.Compression;
using System;

namespace Convention.EasySave.Internal
{
	public static class EasySaveStream
	{
		public static Stream CreateStream(EasySaveSettings settings, EasySaveFileMode fileMode)
		{
            bool isWriteStream = (fileMode != EasySaveFileMode.Read);
            Stream stream = null;

            // Check that the path is in a valid format. This will throw an exception if not.
            new FileInfo(settings.FullPath);

            try
            {
                if (settings.location == EasySave.Location.InternalMS)
                {
                    // There's no point in creating an empty MemoryStream if we're only reading from it.
                    if (!isWriteStream)
                        return null;
                    stream = new MemoryStream(settings.bufferSize);
                }
                else if (settings.location == EasySave.Location.File)
                {
                    if (!isWriteStream && !EasySaveIO.FileExists(settings.FullPath))
                        return null;
                    stream = new EasySaveFileStream(settings.FullPath, fileMode, settings.bufferSize, false);
                }

                return CreateStream(stream, settings, fileMode);
            }
            catch(Exception)
            {
                if (stream != null)
                    stream.Dispose();
                throw;
            }
		}

		public static Stream CreateStream(Stream stream, EasySaveSettings settings, EasySaveFileMode fileMode)
		{
            try
            {
                bool isWriteStream = (fileMode != EasySaveFileMode.Read);

			    #if !DISABLE_ENCRYPTION
                // Encryption
			    if(settings.encryptionType != EasySave.EncryptionType.None && stream.GetType() != typeof(UnbufferedCryptoStream))
			    {
				    EncryptionAlgorithm alg = null;
				    if(settings.encryptionType == EasySave.EncryptionType.AES)
					    alg = new AESEncryptionAlgorithm();
				    stream = new UnbufferedCryptoStream(stream, !isWriteStream, settings.encryptionPassword, settings.bufferSize, alg);
			    }
                #endif

                // Compression
                if (settings.compressionType != EasySave.CompressionType.None && stream.GetType() != typeof(GZipStream))
                {
                    if (settings.compressionType == EasySave.CompressionType.Gzip)
                        stream = isWriteStream ? new GZipStream(stream, CompressionMode.Compress) : new GZipStream(stream, CompressionMode.Decompress);
                }

                return stream;
            }
            catch (System.Exception e)
            {
                if (stream != null)
                    stream.Dispose();
                if (e.GetType() == typeof(System.Security.Cryptography.CryptographicException))
                    throw new System.Security.Cryptography.CryptographicException("Could not decrypt file. Please ensure that you are using the same password used to encrypt the file.");
                else
                    throw;
            }
        }

        public static void CopyTo(Stream source, Stream destination)
        {
        #if UNITY_2019_1_OR_NEWER
            source.CopyTo(destination);
        #else
            byte[] buffer = new byte[2048];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                destination.Write(buffer, 0, bytesRead);
        #endif
        }
	}
}
