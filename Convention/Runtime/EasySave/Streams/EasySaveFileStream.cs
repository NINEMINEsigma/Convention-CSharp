using System.IO;

namespace Convention.EasySave.Internal
{
	public enum EasySaveFileMode {Read, Write, Append}

	public class EasySaveFileStream : FileStream
	{
		private bool isDisposed = false;

		public EasySaveFileStream( string path, EasySaveFileMode fileMode, int bufferSize, bool useAsync)
			: base( GetPath(path, fileMode), GetFileMode(fileMode), GetFileAccess(fileMode), FileShare.None, bufferSize, useAsync)
		{
		}

		// Gets a temporary path if necessary.
		protected static string GetPath(string path, EasySaveFileMode fileMode)
		{
			string directoryPath = EasySaveIO.GetDirectoryPath(path);
            // Attempt to create the directory incase it does not exist if we are storing data.
            if (fileMode != EasySaveFileMode.Read && directoryPath != EasySaveIO.persistentDataPath)
				EasySaveIO.CreateDirectory(directoryPath);
			if(fileMode != EasySaveFileMode.Write || fileMode == EasySaveFileMode.Append)
				return path;
			return (fileMode == EasySaveFileMode.Write) ? path + EasySaveIO.temporaryFileSuffix : path;
		}

		protected static FileMode GetFileMode(EasySaveFileMode fileMode)
		{
			if (fileMode == EasySaveFileMode.Read)
				return FileMode.Open;
			else if (fileMode == EasySaveFileMode.Write)
				return FileMode.Create;
			else
				return FileMode.Append;
		}

		protected static FileAccess GetFileAccess(EasySaveFileMode fileMode)
		{
			if (fileMode == EasySaveFileMode.Read)
				return FileAccess.Read;
			else if (fileMode == EasySaveFileMode.Write)
				return FileAccess.Write;
			else
				return FileAccess.Write;
		}

		protected override void Dispose (bool disposing)
		{
			// Ensure we only perform disposable once.
			if(isDisposed)
				return;
			isDisposed = true;

			base.Dispose(disposing);


			// If this is a file writer, we need to replace the temp file.
			/*if(fileMode == EasySaveFileMode.Write && fileMode != EasySaveFileMode.Append)
			{
				// Delete the old file before overwriting it.
				EasySaveIO.DeleteFile(path);
				// Rename temporary file to new file.
				EasySaveIO.MoveFile(path + EasySave.temporaryFileSuffix, path);
			}*/
		}
	}
}