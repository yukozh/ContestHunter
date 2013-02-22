using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllKorrect
{
    enum MessageType 
    {
        Exit,
        Execute,
        ExecuteReply,
        PutBlob,
        OK,
        GetBlob,
        GetBlobReply,
        MoveBlob2File,
        MoveBlob2Blob,
        MoveFile2File,
        MoveFile2Blob,
        CopyBlob2File,
        CopyBlob2Blob,
        CopyFile2File,
        CopyFile2Blob,
        HasBlob,
        HasFile,
        HasBlobReply,
        HasFileReply
    }
}
