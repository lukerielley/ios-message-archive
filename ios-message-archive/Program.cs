using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ios_message_archive
{
    class Attachment
    {
        public string AttachmentRowId { get; set; }
        public string Filename { get; set; }
        public string MimeType { get; set; }
        public string TransferName { get; set; }
    }

    class Program
    {
        private static SQLiteConnection m_dbConnection;

        static void Main(string[] args)
        {

            // Open the DB

            var smsDb = "3d0d7e5fb2ce288813306e4d4636395e047a3d28";
            var folder = @"S:\@iPHONE_BACKUPS\Luke_2016-05-04_18-50-03\";
            var outputFolder = @"S:\@iPHONE_BACKUPS\output\";
            var outputFolderAttachments = @"S:\@iPHONE_BACKUPS\output\attachments\";

            m_dbConnection = new SQLiteConnection("Data Source=" + folder + smsDb + "; Version=3;");
            m_dbConnection.Open();


            var sql = "SELECT * FROM message where handle_id = '17';";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            var templateFileContent = System.IO.File.ReadAllText(@"S:\GITHUB\lukerielley\ios-message-archive\ios-message-archive\template.html");

            var content = new StringBuilder();

            var placeholderString = "<div id=\"content\"></div>";

            var start = templateFileContent.IndexOf(placeholderString, StringComparison.Ordinal);
            var templateBefore = templateFileContent.Substring(0, start);
            var templateAfter = templateFileContent.Substring(start + placeholderString.Length,
                (templateFileContent.Length - (start + placeholderString.Length)));

            content.AppendLine(templateBefore);

            var count = 0;


            while (reader.Read())
            {
                var ROWID = reader["ROWID"].ToString();

                count++;
                Console.WriteLine("Processing= " + count + ", ROWID= " + ROWID);

                int dtStamp;
                if (int.TryParse(reader["date"].ToString(), out dtStamp))
                {
                    
                    var dt = iOsDateStampToDateTime(dtStamp);

                   
                    content.AppendLine(
                        $"<div class=\"date\"><p>{dt.ToLongDateString()},{dt.ToLongTimeString()}</p></div>");
                   

                    var isFromMe = reader["is_from_me"].ToString() == "1";

                    var isFromMeClass = (isFromMe ? "right" : "left");

                    var service = reader["service"].ToString().ToLower();

                    content.AppendLine(
                        $"<div class=\"{isFromMeClass} {service}\"><blockquote><p>{reader["text"]}</p></blockquote></div>");
                        
                    
                    var attachments = GetAttachments(ROWID);
                    if (attachments.Count > 0)
                    {
                        Console.WriteLine("The message {0} has {1} attachment(s)", ROWID, attachments.Count);
                        foreach (var attachment in attachments)
                        {
                            // get filename
                            var attachmentFilename = SHA1Util.StringToHash(attachment.Filename.Replace("~/", "MediaDomain-"));
                            var attachmentFile = new FileInfo(folder + attachmentFilename);
                            if (attachmentFile.Exists)
                            {
                                Console.WriteLine("Extracting file: " + attachmentFile);
                                var fileExtension = "";
                                switch (attachment.MimeType)
                                {
                                    case "image/png":
                                        fileExtension = ".png";
                                        break;
                                    case "image/jpeg":
                                        fileExtension = ".jpg";
                                        break;
                                    case "image/gif":
                                        fileExtension = ".gif";
                                        break;
                                    case "application/pdf":
                                        fileExtension = ".pdf";
                                        break;
                                    case "text/x-vlocation":
                                        fileExtension = "";
                                        break;
                                    case "text/vcard":
                                        fileExtension = "";
                                        break;
                                    case "video/3gpp":
                                        fileExtension = "";
                                        break;
                                    case "video/mp4":
                                        fileExtension = ".mp4";
                                        break;
                                    case "video/quicktime":
                                        fileExtension = ".mov";
                                        break;
                                }

                                var extractedFilename = ROWID + "-" + attachment.AttachmentRowId + fileExtension;
                                var extractedAttachment = attachmentFile.CopyTo(outputFolderAttachments + extractedFilename);

                                content.AppendLine(
                                    $"<div class=\"{isFromMeClass} {service}\"><img class=\"attachment\" src=\"attachments\\{extractedFilename}\"/></div>");
                            }
                            else
                            {
                                Console.WriteLine("FILE ATTACHMENT IS MISSING!!! :(");
                            }

                            // save file
                        }
                    }
                    
                }
            }


            Console.WriteLine("outputting...");

            content.AppendLine(templateAfter);

            //var output = templateFileContent.Replace("<div id=\"content\"></div>", content.ToString());

            System.IO.File.WriteAllText(outputFolder + "messages.html", content.ToString());

            Console.WriteLine("Done!");
            Console.ReadKey();

        }

        static List<Attachment> GetAttachments(string messageId)
        {
            var attachments = new List<Attachment>();

            var sql = @"
SELECT
	message_attachment_join.message_id,
	attachment.*
FROM 
	attachment
INNER JOIN
	message_attachment_join
ON
	attachment.ROWID = message_attachment_join.attachment_id
WHERE
	message_attachment_join.message_id = '" + messageId + "'";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                attachments.Add(new Attachment()
                {
                    AttachmentRowId = reader["ROWID"].ToString(),
                    Filename = reader["filename"].ToString(),
                    MimeType = reader["mime_type"].ToString(),
                    TransferName = reader["transfer_name"].ToString()
                });
            }

            return attachments;
        }

        static DateTime iOsDateStampToDateTime(int timestamp)
        {
            var iOSEpoch = new DateTime(2001, 01, 01);
            return iOSEpoch.AddSeconds(timestamp).ToLocalTime();
        }

    }
}
