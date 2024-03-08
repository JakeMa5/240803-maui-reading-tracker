namespace ReadingTracker
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzExNjM2OEAzMjM0MmUzMDJlMzBRK3duZHI2bmxLcFpUZGJua1U3WXFIRHl0S2RoM3hxVEdqYzNFS2ZOUUhnPQ==");
            InitializeComponent();

            Application.Current.UserAppTheme = AppTheme.Light;

            MainPage = new AppShell();

        }
    }
}