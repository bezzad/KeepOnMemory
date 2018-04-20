using System;
using System.Text;

namespace CloudStorage
{
    public class RowItem
    {
        public RowItem(string fileName, string fileType, long fileSize, string fileAddress)
        {
            Name = fileName;
            Extension = fileType;
            longSize = fileSize;
            Address = fileAddress;
            Selection = false;
        }
        public RowItem(string fileName, string fileType, long fileSize, string fileAddress, byte[] fileData)
        {
            Name = fileName;
            Extension = fileType;
            Address = fileAddress;
            Data = fileData;
            longSize = fileSize;
            Selection = false;
        }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string stringSize { get { return GetBreakingSize(this.longSize); } }
        public string Address { get; set; }
        public long longSize { get; set; }
        public byte[] Data { get; set; }
        public Nullable<bool> Selection { get; set; }
        public static string GetBreakingSize(double length)
        {
            string strSize = null;
            //
            // Convert Bytes to Larger Unit:
            // If GigaByte:
            if ((length / 1024 / 1024 / 1024) >= 1)
            {
                strSize = String.Format("{0:0.###}", (length / 1024 / 1024 / 1024)) + " GB";
            }
            //
            // If MegaByte:
            else if ((length / 1024 / 1024) >= 1)
            {
                strSize = String.Format("{0:0.###}", (length / 1024 / 1024)) + " MB";
            }
            //
            // If KiloByte:
            else if ((length / 1024) >= 1)
            {
                strSize = String.Format("{0:0.###}", (length / 1024)) + " KB";
            }
            //
            // Else = Bytes
            else
            {
                strSize = String.Format("{0:0.###}", length) + " Bytes";
            }
            return strSize;
        }
    }
}
