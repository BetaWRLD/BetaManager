﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32.SafeHandles;

using MonoTorrent.Client;

using ReusableTasks;

namespace MonoTorrent.PieceWriter
{
    interface IFileReaderWriter : IDisposable
    {
        bool CanWrite { get; }

        ReusableTask FlushAsync ();
        ReusableTask<int> ReadAsync (Memory<byte> buffer, long offset);
        ReusableTask WriteAsync (ReadOnlyMemory<byte> buffer, long offset);
    }

    static class FileReaderWriterHelper
    {
        public static void MaybeTruncate (string fullPath, long length)
        {
            var fileStream = new System.IO.FileStream (fullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite, 1, FileOptions.None);
            if (fileStream.Length <= length) {
                fileStream.Dispose ();
                return;
            }

            fileStream.Dispose ();
            fileStream = new FileStream (fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 1, FileOptions.None);
            fileStream.SetLength (length);
            fileStream.Dispose ();
        }
    }

    class RandomFileReaderWriter : IFileReaderWriter
    {
#if NET6_0_OR_GREATER
        SafeFileHandle Handle { get; }
#else
        FileStream Handle { get; }
#endif
        public bool CanWrite { get; }
        public long Length { get; }

        public RandomFileReaderWriter (string fullPath, long length, FileMode fileMode, FileAccess access, FileShare share)
        {
            FileReaderWriterHelper.MaybeTruncate (fullPath, length);
#if NET6_0_OR_GREATER
            Handle = File.OpenHandle (fullPath, fileMode, access, share, FileOptions.None);
#else
            Handle = new FileStream (fullPath, fileMode, access, share, 1, FileOptions.None);
#endif
            CanWrite = access.HasFlag (FileAccess.Write);
            Length = length;
        }

        public void Dispose ()
        {
            Handle.Dispose ();
        }

#if NET6_0_OR_GREATER
        public ReusableTask FlushAsync ()
            => ReusableTask.CompletedTask;
#else
        public async ReusableTask FlushAsync ()
        {
            await new ThreadSwitcher ();
            Handle.Flush ();
        }
#endif

        public async ReusableTask<int> ReadAsync (Memory<byte> buffer, long offset)
        {
            if (offset + buffer.Length > Length)
                throw new ArgumentOutOfRangeException (nameof (offset));

            await new ThreadSwitcher ();
#if NET6_0_OR_GREATER
            return RandomAccess.Read (Handle, buffer.Span, offset);
#else
            if (Handle.Position != offset)
                Handle.Seek (offset, SeekOrigin.Begin);
            return Handle.Read (buffer);
#endif
        }

        public async ReusableTask WriteAsync (ReadOnlyMemory<byte> buffer, long offset)
        {
            if (offset + buffer.Length > Length)
                throw new ArgumentOutOfRangeException (nameof (offset));

            await new ThreadSwitcher ();
#if NET6_0_OR_GREATER
            RandomAccess.Write (Handle, buffer.Span, offset);
#else
            if (Handle.Position != offset)
                Handle.Seek (offset, SeekOrigin.Begin);
            Handle.Write (buffer);
#endif
        }
    }
}
