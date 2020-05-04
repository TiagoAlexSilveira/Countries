using Countries.Modelos;
using Countries.Services;
using Countries.Serviços;
using Microsoft.Win32;
using Newtonsoft.Json;
using NReco.ImageGenerator;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Countries
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Country> country_;  //só precisamos da referência da Lista(só é utilizada uma vez)        

        private NetworkService networkService;

        private ApiService apiService;

        private DialogService dialogService;


        public MainWindow()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            LoadCountries();
            SaveFlagASync(country_);


        }

        private async void LoadCountries()
        {
            var connection = networkService.CheckConnection(); //faz a conexão à net

            if (!connection.IsSucess)
            {
                //Load Local Database se não tiver net carregamos a base de dados local
            }
            else
            {
                await LoadApiCountries();
            }
        }



        private async Task LoadApiCountries()
        {
            var response = await apiService.GetCountries("http://restcountries.eu", "/rest/v2/all");

            country_ = (List<Country>)response.Result;
            MessageBox.Show(country_.Count.ToString());

            foreach (Country country in country_)
            {
                lb_countries.Items.Add(country.name);
            }
        }

        public async Task SaveFlagASync(List<Country> countries)
        {

            //WebClient client = new WebClient();

            //foreach (Country country in country_)
            //{
            //    client.DownloadFile(new Uri(country.flag), "C:\\Users\\Tiago\\source\\repos\\Countries\\Countries\\FlagsImg");
            //}

            using(WebClient client = new WebClient())
            {
                NReco.ImageGenerator.HtmlToImageConverter ConvImg = new NReco.ImageGenerator.HtmlToImageConverter();

                foreach (Country country in country_)
                {
                    client.DownloadFile(new Uri(country.flag), $"C:\\Users\\Tiago\\source\\repos\\Countries\\Countries\\FlagsImg\\{country.name}.jpg)");
                    client.DownloadFile(country.flag, $"C:\\Users\\Tiago\\source\\repos\\Countries\\Countries\\FlagsImg\\{country.name}.svg");

                    ConvImg.Width = 440;
                    ConvImg.Height = 320;
                    var FlagImage = ConvImg.GenerateImageFromFile(country.flag, ImageFormat.Jpeg);

                    using (var stream = new System.IO.MemoryStream(FlagImage, 0, FlagImage.Length))
                    {
                        Bitmap bm = new Bitmap(System.Drawing.Image.FromStream(stream));
                        bm.Save(country.name, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
            }           
        }





        private void lb_countries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Country country in country_)
            {
                if (country.name == lb_countries.SelectedItem.ToString())
                {
                    lbl_capital.Content = country.capital;
                    lbl_region.Content = country.region;
                    lbl_subregion.Content = country.subregion;
                    lbl_population.Content = country.population;

                    if (country.capital == "")
                    {
                        lbl_capital.Content = "N/A";
                    }

                    if (country.subregion == "")
                    {
                        lbl_subregion.Content = "N/A";
                    }
                }
            }
        }
    }
}
