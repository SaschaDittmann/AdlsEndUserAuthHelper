using System;
using System.Windows;
using System.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AdlsEndUserAuthHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string resource = ConfigurationManager.AppSettings["Resource"];
        static string redirectUrl = ConfigurationManager.AppSettings["RedirectUrl"];

        public MainWindow()
        {
            InitializeComponent();
            ClientIdTextBox.Text = ConfigurationManager.AppSettings["ClientId"];
            TenantIdTextBox.Text = ConfigurationManager.AppSettings["TenantId"];
        }
        
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tenantId = TenantIdTextBox.Text;
                var clientId = ClientIdTextBox.Text;

                AuthenticationContext ctx = 
                    new AuthenticationContext($"https://login.windows.net/{tenantId}/");

                var authResult = await ctx.AcquireTokenAsync(
                    resource,
                    clientId,
                    new Uri(redirectUrl),
                    DisableSso.IsChecked.Value ? PromptBehavior.Always : PromptBehavior.Auto,
                    UserIdentifier.AnyUser,
                    null,
                    null
                    );

                AccessTokenTextBox.Text = authResult.AccessToken;
                RefreshTokenTextBox.Text = authResult.RefreshToken;
                SparkConfTextBox.Text = $"spark.conf.set(\"dfs.adls.oauth2.access.token.provider.type\", \"RefreshToken\")\n"
                    + $"spark.conf.set(\"dfs.adls.oauth2.client.id\", \"{clientId}\")\n"
                    + $"spark.conf.set(\"dfs.adls.oauth2.refresh.token\", \"{authResult.RefreshToken}\")\n"
                    + $"spark.conf.set(\"dfs.adls.oauth2.refresh.url\", \"https://login.windows.net/{authResult.TenantId}/oauth2/token\")";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CopyAccessToken_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AccessTokenTextBox.Text);
            AccessTokenTextBox.SelectAll();
            AccessTokenTextBox.Focus();
        }

        private void CopyRefreshToken_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(RefreshTokenTextBox.Text);
            RefreshTokenTextBox.SelectAll();
            RefreshTokenTextBox.Focus();
        }

        private void CopySparkConf_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(SparkConfTextBox.Text);
            SparkConfTextBox.SelectAll();
            SparkConfTextBox.Focus();
        }
    }
}
