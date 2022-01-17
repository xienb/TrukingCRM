using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Truking.CRM.Web.Model
{
    [Serializable]
    public class Chunk
    {
        private Int64 id;
        /**
         * 当前文件块，从1开始
         */
        private Int64 chunkNumber;
        /**
         * 分块大小
         */
        private Int64 chunkSize;
        /**
         * 当前分块大小
         */
        private Int64 currentChunkSize;
        /**
         * 总大小
         */
        private Int64 totalSize;
        /**
         * 文件标识
         */
        private String identifier;
        /**
         * 文件名
         */
        private String filename;
        /**
         * 相对路径
         */
        private String relativePath;
        /**
         * 总块数
         */
        private int totalChunks;
        /**
         * 文件类型
         */
        private String type;

        //private MultipartFile file;
    }
}