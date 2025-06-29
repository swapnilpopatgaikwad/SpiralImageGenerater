using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Upload;
using File = Google.Apis.Drive.v3.Data.File;

namespace SpiralImageGenerater
{
    public static class GoogleUploader
    {
        private static string[] Scopes = {
        DriveService.Scope.DriveFile,
        SheetsService.Scope.Spreadsheets
    };
        private static string ApplicationName = "My Drive Sheets App";
        private static string SpreadsheetId = "";
        private static string SheetName = "";
        private static string DevWithSwapSpreadsheetId = "1H8oZsFC-SC-qKqfg_8pxESfm7haXdEVi2Mwz9XnEibs";
        private static string MangabharaSpreadsheetId = "1AnWTpK-2K8v5mphDIV6bA6djZRdLtB2fBVDxxJDjjpY";
        private static string BalManacheMotiSpreadsheetId = "1rkgdWSsN7emUIlb665iKREWDQOO_VQKNEOqeOX8TsAU";
        private static DriveService driveService;
        private static SheetsService sheetsService;

        public static void Init(BrandType brandType)
        {
            UserCredential credential;
            GoogleAuthorizationCodeFlow.Initializer initializer;

            switch(brandType)
            {
                case BrandType.DevWithSwap:
                    SpreadsheetId = DevWithSwapSpreadsheetId;
                    SheetName = "DevWithSwap";
                    break;
                case BrandType.Mangabhara:
                    SpreadsheetId = MangabharaSpreadsheetId;
                    SheetName = "Mangabhara";
                    break;
                case BrandType.BalManacheMoti:
                    SpreadsheetId = BalManacheMotiSpreadsheetId;
                    SheetName = "BalManacheMoti";
                    break;
                default:
                    throw new ArgumentException("Invalid brand type");
            }

            string currentDir = Directory.GetCurrentDirectory();
            string projectPath = Directory.GetParent(currentDir)?.Parent?.Parent?.FullName ?? currentDir;
            string finalPath = Path.Combine(projectPath, "credentials.json");
            using (var stream = new FileStream(finalPath, FileMode.Open, FileAccess.Read))
            {
                var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

                initializer = new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = secrets,
                    Scopes = Scopes
                };
            }

            // Append `prompt=select_account` to force account selection
            var flow = new GoogleAuthorizationCodeFlow(initializer);

            var codeReceiver = new LocalServerCodeReceiver();

            credential = new AuthorizationCodeInstalledApp(flow, codeReceiver)
                .AuthorizeAsync("user", CancellationToken.None).Result;


            driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        private static string UploadImageToDrive(string filePath, string folderId = null)
        {
            var fileMetadata = new File()
            {
                Name = Path.GetFileName(filePath),
                Parents = folderId != null ? new[] { folderId } : null
            };

            using var stream = new FileStream(filePath, FileMode.Open);
            var request = driveService.Files.Create(fileMetadata, stream, "image/png");
            request.Fields = "id";
            var file = request.Upload();

            if (file.Status != UploadStatus.Completed)
                throw new Exception("Upload failed");

            string fileId = request.ResponseBody.Id;

            // Make file public
            var permission = new Permission
            {
                Type = "anyone",
                Role = "reader"
            };
            driveService.Permissions.Create(permission, fileId).Execute();

            return $"https://drive.google.com/uc?id={fileId}";
        }

        private static void AppendToSheet(string spreadsheetId, string sheetName, string imageName, string thought, string driveLink)
        {
            var range = $"{sheetName}!A:C";
            var valueRange = new ValueRange
            {
                Values = new List<IList<object>> {
                new List<object> { imageName, thought, driveLink }
            }
            };

            var appendRequest = sheetsService.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            appendRequest.Execute();
        }

        public static void SaveImageToDriveAndSheet(ImageModel imageModel)
        {
            if (!System.IO.File.Exists(imageModel.ImagePath))
            {
                Console.WriteLine("Image not found: " + imageModel.ImagePath);
                return;
            }
            string driveLink = UploadImageToDrive(imageModel.ImagePath);
            string imageName = Path.GetFileName(imageModel.ImagePath);
            AppendToSheet(SpreadsheetId, SheetName, imageName, imageModel.ThoughtText, driveLink);
            Console.WriteLine($"Uploaded: {imageName} -> {driveLink}");
        }
    }
}
