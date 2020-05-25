using Countries.Modelos;
using Countries.Services;
using Countries.Serviços;
using Microsoft.Win32;
using Newtonsoft.Json;
using NReco.ImageGenerator;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private List<Country> paises;  //só precisamos da referência da Lista(só é utilizada uma vez)        

        private NetworkService networkService;

        private ApiService apiService;

        private DialogService dialogService;


        public MainWindow()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            paises = new List<Country>();
            LoadCountries();
            this.DataContext = lb_countries;
            
        }

        private async void LoadCountries()
        {
            await LoadApiCountries();
            lb_countries.ItemsSource = paises;
            lb_countries.DisplayMemberPath = "name";
            await SaveFlagASync(paises);
            //await ConvertSvgAsync();

            //MessageBox.Show(paises.Count.ToString());
            GetContinent();
        }



        private async Task LoadApiCountries()
        {
            var response = await apiService.GetCountries("http://restcountries.eu", "/rest/v2/all");

            paises = (List<Country>)response.Result;
        }


        public async Task SaveFlagASync(List<Country> countries)
        {
            DirectoryInfo path = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent;

            List<Task> tasks = new List<Task>();

            if (!Directory.Exists(path.FullName + @"\FlagsImg\"))
            {
                Directory.CreateDirectory(path.FullName + @"\FlagsImg\");
            }

            string FullPath = path.FullName + @"\FlagsImg\";

            foreach (Country country in countries)
            {
                //tasks.Add(Task.Run(() => DownloadSvg(FullPath, country)));
                tasks.Add(Task.Run(() => DownloadSvg(FullPath, country)));
            }
            
            await Task.WhenAll(tasks);
        }

        private void DownloadSvg(string fullpath, Country country)
        {
            //linha adicionada no xaml
            //try catch
            WebClient client = new WebClient();

            client.DownloadFile(new Uri(country.flag), $"{fullpath}{country.name}.svg");

            country.FlagImgPath = $"{fullpath}{country.name}.svg";

            client.Dispose();
        }

        private void lb_countries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (lb_countries.SelectedItem != null)
            {
                Country country = (Country)lb_countries.SelectedItem;

                lbl_country.Content = country.name;
                lbl_capital.Content = country.capital;
                lbl_region.Content = country.region;
                lbl_subregion.Content = country.subregion;
                lbl_population.Content = country.population;
                lbl_gini.Content = country.gini;

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


        private void GetContinent()
        {
            //vai buscar todos os elementos distintos da propriedade region da lista de paises
            var continents = paises.Select(c => c.region).Distinct().ToList();

            cb_continents.ItemsSource = continents;
        }


        private void cb_continents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Country> tempcountry;

            //encontra todos os países que que têm a region igual ao da selecionada na combo box
            tempcountry = paises.FindAll(c => c.region.Equals(cb_continents.SelectedItem));

            lb_countries.ItemsSource = null;
            lb_countries.ItemsSource = tempcountry;
            lb_countries.DisplayMemberPath = "name";
        }
    }
}
