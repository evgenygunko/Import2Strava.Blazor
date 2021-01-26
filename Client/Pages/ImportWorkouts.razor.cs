using System.Linq;
using System.Threading.Tasks;
using MatBlazor;

namespace Client.Pages
{
    public partial class ImportWorkouts
    {
        public string Error { get; set; }

        public string SelectedFile { get; set; }

        public bool IsFileNotSelected { get => string.IsNullOrEmpty(SelectedFile); }

        public bool IsUploading { get; set; }

        public bool IsParsing { get; set; }

        private void FilesReady(IMatFileUploadEntry[] files)
        {
            Error = null;
            SelectedFile = null;

            IMatFileUploadEntry file = files.FirstOrDefault();
            if (file == null)
            {
                return;
            }

            if (!file.Name.EndsWith(".zip", System.StringComparison.OrdinalIgnoreCase))
            {
                Error = "Only zip file can be uploaded";
            }

            SelectedFile = file.Name;
        }

        private async Task UploadArchiveAsync()
        {
            IsUploading = true;
            StateHasChanged();

            //try
            //{
            //    Error = null;
            //    await DataService.UnlinkStravaAppAsync();

            //    User.IsStravaAccountLinked = false;
            //}
            //catch (Exception ex)
            //{
            //    Error = "An error occurred: " + ex;
            //}
            await Task.Delay(5000);

            IsUploading = false;

            IsParsing = true;
            StateHasChanged();

            await Task.Delay(5000);

            IsParsing = false;
        }
    }
}
