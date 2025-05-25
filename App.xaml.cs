namespace CollectionManagerApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine($"App Data Path: {FileSystem.AppDataDirectory}");
            MainPage = new AppShell();
        }
    }
}
