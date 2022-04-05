﻿using MTG.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MTG.ViewModel
{
    public class CardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public CardModel _cardModel { get; set; }

        public List<CardModel> _allCards;

        public ObservableCollection<CardModel> Cards { get; set; }

        //for binding
        public string CardName { get; set; }
        public BitmapImage CardImageSource { get; set; }
        private Uri _imageUri;
        public int[] cardsIndex =new int[5];
        public string CardColor { get; set; }
        public string BackgroundColor { get; set; }
        string color { get; set; }
        string color2 { get; set; }
        public ImageSource Source { get; set; }

        private CardModel _selectedNote;

        public CardModel SelectedNote
        {
            get { return _selectedNote; }

            set
            {
                _selectedNote = value;

                if (value == null)
                {
                    CardName = "";
                    CardImageSource = null;
                    CardColor = "White";
                    BackgroundColor = "White";
                }
                else
                {                    
                    CardName = value.Name;
                    _imageUri = new Uri(value.Image);
                    CardImageSource = new BitmapImage(_imageUri);
                    CardColor = value.Color;
                    BackgroundColor = value.Color2;
                   
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CardName"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CardImageSource"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CardColor"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BackgroundColor"));
            }
        }

        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set
            {
                if (value == _filter) { return; }

                _filter = value;

                PerformFiltering();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filter)));
            }
        }

        public CardViewModel()
        {
            Cards = new ObservableCollection<CardModel>();
            _allCards = new List<CardModel> ();
            GetCards(1000);
            PerformFiltering();

        }


        public async void GetCards(int number)
        {
            Uri baseURI = new Uri("https://api.magicthegathering.io/v1/cards/");
            Uri uri = new Uri(baseURI.ToString());
            HttpClient httpClient = new HttpClient();
            var result = await httpClient.GetStringAsync(uri);
            JObject data = (JObject)JsonConvert.DeserializeObject(result);
            int count = data["cards"].Count();
            Random rm = new Random();
            

            do {
                int index = rm.Next(count);
                JToken test = data["cards"][index]["imageUrl"] as JToken;
                if (test != null)
                {
                

                    string name = data["cards"][index]["name"].ToString();
                    string image = data["cards"][index]["imageUrl"].ToString();
                    string checkColor = data["cards"][index]["colors"][0].ToString();

                    switch (checkColor) {
                        case "Blue":
                            color = "#0075BD";                           
                            color2 = "#CDEEFD";
                            break;
                        case "White":
                            color = "#F6E9D2";
                            color2 = "#fffdeb";                          
                            break;
                        case "Black":
                            color = "#3D3D3D";                        
                            color2 = "LightGray";
                            break;
                        case "Green":
                            color = "#228C22";
                            color2 = "#228C22";                         
                            break;
                    }
                 
                    CardModel card = new CardModel(name, image, color, color2);
                    _allCards.Add(card);
                    Cards.Add(card);
                    number--;
                }
                else {
                  
                }

                } while (number>0);
        }

        private void PerformFiltering()
        {
            if (_filter == null)
            {
                _filter = "";
            }

            var lowerCaseFilter = Filter.ToLowerInvariant().Trim();

            var result =
                _allCards.Where(d => d.Name.ToLowerInvariant()
                .Contains(lowerCaseFilter))
                .ToList();

            var toRemove = Cards.Except(result).ToList();

            foreach (var x in toRemove)
            {
                Cards.Remove(x);
            }

            var resultCount = result.Count;

            for (int i = 0; i < resultCount; i++)
            {
                var resultItem = result[i];
                if (i + 1 > Cards.Count || !Cards[i].Equals(resultItem))
                {
                    Cards.Insert(i, resultItem);
                }
            }
        }

    }
}
