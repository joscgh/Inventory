namespace Inventory.Native
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            if (sender is RefreshView refreshView)
            {
                refreshView.IsRefreshing = false;
            }
        }
    }
}
