﻿using Countries.Modelos;
using Countries.Models;
using Countries.Services;
using Countries.Serviços;
using Microsoft.Win32;
using Newtonsoft.Json;
using NReco.ImageGenerator;
using Svg;
using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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

        private List<LivingCost> LiveCosts;

        private List<Rate> Rates;

        private NetworkService networkService;

        private ApiService apiService;

        private DialogService dialogService;

        //private DataService dataService;


        public MainWindow()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            paises = new List<Country>();
            LiveCosts = new List<LivingCost>();
            LoadInfo();
            this.DataContext = lb_countries;
        }

        private async void LoadInfo()
        {
            bool load;

            tb_status.Text = "Retrieving countries... ";

            var connection = networkService.CheckConnection(); //verificar a conexão

            if (!connection.IsSucess)     // se não houver conexão à internet
            {
                //LoadLocalCountries();  //Implementar BASE DE DADOS********
                load = false;
            }
            else      //se houver conexão à internet
            {
                await LoadApiCountries();

                AddLinetxt(tb_status, "Countries sucessfully loaded!");
                tb_status.ScrollToEnd();

                lb_countries.ItemsSource = paises;
                lb_countries.DisplayMemberPath = "name";

                await SaveFlagASync(paises);
                AddLinetxt(tb_status, "Countries sucessfully saved!");
                tb_status.ScrollToEnd();

                GetContinent();

                //await LoadApiRates();
                await LoadApiLivingCosts();


                load = true;

            }


            if (load) //se o load for true é porque carregou da internet
            {
                AddLinetxt(tb_status, string.Format("Taxas carregadas da Internet em {0:F}", DateTime.Now));
                tb_status.ScrollToEnd();
            }
            else //senão carregou sem internet e teve de carregar da base de dados local
            {
                AddLinetxt(tb_status, string.Format("Taxas carregadas da Base de Dados."));
            }
        }


        //private async void LoadRates()   //para a aplicação não deixar de correr enquanto faz load às taxas inserimos async(e await em baixo)
        //{
        //    bool load;

        //    AddLinetxt(tb_status, "Updating rates...");

        //    var connection = networkService.CheckConnection(); //verificar a conexão

        //    if (!connection.IsSucess)     // se não houver conexão à internet
        //    {
        //        //LoadLocalRates();
        //        load = false;
        //    }
        //    else      //se houver conexão à internet
        //    {
        //        await LoadApiRates();
        //        load = true;
        //    }

        //    ////Se se ligar o programa pela primeira vez sem internet, a base de dados está vazia por isso metwemos este if
        //    //if (Rates.Count == 0) // se a lista de rates estiver vazia
        //    //{
        //    //    LabelResultado.Text = "Não há ligação à Internet" + Environment.NewLine +
        //    //        "e não foram previamente careegadas as taxas" + Environment.NewLine +
        //    //        "Tente mais tarde";

        //    //    LabelStatus.Text = "Primeira inicialização deverá ter ligação à internet";

        //    //    return;
        //    //}

        //    //ComboBoxOrigem.DataSource = Rates;
        //    //ComboBoxOrigem.DisplayMember = "Name";

        //    ////Corrige bug da microsoft
        //    //ComboBoxDestino.BindingContext = new BindingContext(); // ComboBoxDestino tem um binding diferente que a ComboBoxOrigem
        //    //                                                       // é necessário fazer este passo porque as duas ComboBox estão sempre a mostrar o mesmo valor

        //    //ComboBoxDestino.DataSource = Rates;
        //    //ComboBoxDestino.DisplayMember = "Name";

        //    //LabelResultado.Text = "Taxas atualizadas...";

        //    if (load) //se o load for true é porque carregou da internet
        //    {
        //         AddLinetxt(tb_status, string.Format("Taxas carregadas da Internet em {0:F}", DateTime.Now));
        //         tb_status.ScrollToEnd();
        //    }
        //    else //senão carregou sem internet e teve de carregar da base de dados local
        //    {
        //        AddLinetxt(tb_status, string.Format("Taxas carregadas da Base de Dados."));
        //    }

        //    //progressBar1.Value = 100;

        //    //ButtonConverter.Enabled = true;
        //    //ButtonTroca.Enabled = true;
        //}


        private async Task LoadApiCountries()
        {
            var response = await apiService.GetCountries("http://restcountries.eu", "/rest/v2/all");

            paises = (List<Country>)response.Result;
        }

        private async Task LoadApiLivingCosts() //Consegui por isto a carregar o que quero mas demora 4 minutos a carregar tudo, tem muitas chamadas à api
        {
            Stopwatch st = new Stopwatch();
            st.Start();  
            MessageBox.Show("está a começar o load costs ");

            foreach (Country country in paises)
            {
                var capital = country.capital.ToString();

                var response = await apiService.GetCosts("https://cost-of-living-api-lqvgibwbps.now.sh", ($"/{capital}"));

                //var response = await apiService.GetCosts("https://cost-of-living-api-lqvgibwbps.now.sh", "/Lisbon");

                LivingCost cost = (LivingCost)response.Result;
                LiveCosts.Add(cost);
                //LiveCosts.Add((LivingCost)response.Result);
            }

            //var response = await apiService.GetCosts("https://cost-of-living-api-lqvgibwbps.now.sh", "/Lisbon");

            //LivingCost cost = (LivingCost)response.Result;
            //LiveCosts.Add(cost);

            st.Stop();            
            MessageBox.Show(st.ElapsedMilliseconds.ToString());

            MessageBox.Show(LiveCosts.Count().ToString());
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

                MessageBox.Show(LiveCosts.Count().ToString());

                //Mudar isto
                if (country.capital != null)
                {
                    foreach (LivingCost costts in LiveCosts)
                    {
                        if (costts != null)
                        {
                            if ((costts.city.ToString()) == (country.capital.ToString()))
                            {
                                MessageBox.Show("Encontrei uma cidade com o mesmo nome que a capital");
                                if (costts.costs != null && costts.costs.Count != 0)
                                {                                   
                                    //var livingcost = LiveCosts.Find(x => x.city == country.capital);

                                    lbl_aptcity.Content = ConvertEUR(costts.costs[47].range.high);
                                    lbl_aptout.Content = ConvertEUR(costts.costs[48].range.high);
                                    lbl_bn.Content = ConvertEUR(costts.costs[35].cost);
                                    lbl_water.Content = ConvertEUR(costts.costs[7].cost);
                                    lbl_gas.Content = ConvertEUR(costts.costs[32].cost);
                                    lbl_trpt.Content = ConvertEUR(costts.costs[28].cost);
                                    lbl_net.Content = ConvertEUR(costts.costs[37].cost);
                                    lbl_cigar.Content = ConvertEUR(costts.costs[26].cost);
                                    lbl_city.Content = costts.city;
                                } 
                            }
                        }
                    }



                    //if (check != null)
                    //{
                    //    //LiveCosts.Any(x => x.city.ToString().Contains(country.capital.ToString())
                    //    //LivingCost costt = LiveCosts.Find(x => x.city == country.capital);
                    //    //var check = LiveCosts.Select(x => x.city == country.capital);
                    //    //var check = LiveCosts.Where(x => x.city.Equals(country.capital)).First();
                    //    //var check = LiveCosts.Where(x => x.city.Equals(country.capital));
                    //    //var check = LiveCosts.FirstOrDefault(x => x.city == country.capital);
                    //}
                    //else
                    //{
                    //    MessageBox.Show("NÃO encontrei uma cidade com o mesmo nome que a capital");
                    //}


                    //ListCost.DataContext = livingcost.costs;
                    //lbl_1.DataContext = livingcost.costs[2];
                }




                //var capital = paises.FindAny(d => d.capital)

                //var capital = LiveCosts.Where(a => paises.Any(x => x.capital == a.city));

                //LivingCost livcost = new LivingCost();

                //foreach (LivingCost cos in LiveCosts)
                //{
                //    if (country.capital.ToString() == cos.city.ToString() && cos.city != null)
                //    {
                //        livcost = cos;
                //        MessageBox.Show(livcost.city);
                //    }
                //}


                //var cost = LiveCosts.Where(p => paises.Any(l => p.city == l.capital));

                //MessageBox.Show(cost.ToString());

                //var costs = LiveCosts.Find(p => p.city == country.capital);               


                //string test = GetWebsite(country.name);  era para fazer webscraping mas não vou fazer
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


        private async Task LoadApiRates()
        {
            //progressBar1.Value = 0;

            var response = await apiService.GetRates("https://cambiosrafa.azurewebsites.net", "/api/rates");
            //como chamamos um método com async para que o método continue a funcionar, precisamos de meter await

            Rates = (List<Rate>)response.Result; //converter para uma lista de Rates

            //DataService.DeleteData();  IMPLEMENTAR DEPOIS PARA GUARDAR NA BASE DE DADOS*******************

            //dataService.SaveData(Rates);
        }

        //Método para adicionar uma linha nova à caixa de texto
        public void AddLinetxt(TextBox textbox, string text)
        {
            textbox.AppendText("\r\n" + text);
        }


        //Converter valores de uma string em dollar canadiano para euro
        public double ConvertEUR(string cad)
        {
            if (cad != null && cad != "?")
            {
                var cadnum = double.Parse(cad, CultureInfo.InvariantCulture);
                var cd = Math.Round((cadnum * 0.66) * 100) / 100;

                return cd;
            }

            return 0;
        }






        //era para ser utilizado para web scraping mas não vou usar
        //guarda o html do site inserido numa variável
        //private string GetWebsite(string countryname)
        //{
        //    var _plainText = string.Empty;
        //    var _request = (HttpWebRequest)WebRequest.Create($"https://www.expatistan.com/cost-of-living/country/{countryname}");
        //    _request.Timeout = 5000;
        //    _request.Method = "GET";
        //    _request.ContentType = "text/plain";
        //    using (var _webResponse = (HttpWebResponse)_request.GetResponse())
        //    {
        //        var _webResponseStatus = _webResponse.StatusCode;
        //        var _stream = _webResponse.GetResponseStream();
        //        using (var _streamReader = new StreamReader(_stream))
        //        {
        //            _plainText = _streamReader.ReadToEnd();
        //        }
        //    }

        //    return _plainText;
        //}
    }
}
