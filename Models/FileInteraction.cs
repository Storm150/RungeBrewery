﻿using BrewUI.Data;
using BrewUI.Items;
using Microsoft.SqlServer.Server;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BrewUI.Models
{
    public class FileInteraction
    {
        public static BreweryRecipe OpenRecipe()
        {
            BreweryRecipe BR = new BreweryRecipe();
            string recipeText = "";
            
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Recipe (*.br)|*.br";
            openDialog.FilterIndex = 1;

            try
            {
                if (openDialog.ShowDialog() == true)
                {
                    recipeText = File.ReadAllText(openDialog.FileName);
                }
                else
                {
                    return BR;
                }
            }
            catch
            {
                MessageBox.Show("Recipe file is corrupted and cannot be opened.", "Error");
                return BR;
            }

            #region Read session info

            SessionInfo si = new SessionInfo();
            string siText = GetDataInbetween("<SESSIONINFO>", recipeText);

            // Read name
            si.sessionName = GetDataInbetween("<NAME>", siText);

            // Read batch size
            si.BatchSize = Convert.ToDouble(GetDataInbetween("<BATCHSIZE>", siText));

            // Read brew method
            si.BrewMethod = GetDataInbetween("<BREWMETHOD>", siText);

            // Read beer style
            BeerStyle bs = new BeerStyle();
            bs.Name = GetDataInbetween("<STYLE>", siText);
            si.style = bs;

            #endregion

            #region Read grains

            ObservableCollection<Grain> grains = new ObservableCollection<Grain>();
            string grainsText = GetDataInbetween("<GRAINLIST>", recipeText);
            string[] grainsArray = grainsText.Split(new string[] { "<GRAIN>" }, StringSplitOptions.RemoveEmptyEntries);

            foreach(string grainText in grainsArray)
            {
                if (grainText.Contains("</GRAIN>"))
                {
                    Grain grain = new Grain();
                    
                    // Read name
                    grain.name = GetDataInbetween("<NAME>", grainText);

                    // Read amount
                    grain.amount = Convert.ToDouble(GetDataInbetween("<AMOUNT>", grainText));

                    grains.Add(grain);
                }
            }

            #endregion

            #region Read mash steps

            ObservableCollection<MashStep> mashSteps = new ObservableCollection<MashStep>();
            string mashText = GetDataInbetween("<MASHSTEPS>", recipeText);
            string[] mashArray = mashText.Split(new string[] { "<MASHSTEP>" }, StringSplitOptions.RemoveEmptyEntries);

            foreach(string mt in mashArray)
            {
                if (mt.Contains("</MASHSTEP>"))
                {
                    MashStep ms = new MashStep();

                    //Read name
                    ms.stepName = GetDataInbetween("<NAME>", mt);

                    // Read temperature
                    ms.stepTemp = Convert.ToDouble(GetDataInbetween("<TEMPERATURE>", mt));

                    // Read duration
                    ms.stepDuration = TimeSpan.FromMinutes(Convert.ToDouble(GetDataInbetween("<DURATION>", mt)));

                    mashSteps.Add(ms);
                }
            }
            #endregion

            #region Read sparge

            SpargeStep ss = new SpargeStep();
            string spargeText = GetDataInbetween("<SPARGESTEP>", recipeText);
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ",";
            ss.spargeTemp = Convert.ToDouble(GetDataInbetween("<TEMPERATURE>", spargeText),provider);
            ss.spargeWaterAmount = Convert.ToDouble(GetDataInbetween("<AMOUNT>", spargeText),provider);
            //ss.spargeDur = TimeSpan.FromMinutes(Convert.ToDouble(GetDataInbetween("<DURATION>", spargeText)));

            #endregion

            #region Read hops

            ObservableCollection<Hops> hopsList = new ObservableCollection<Hops>();
            string hopsText = GetDataInbetween("<HOPSLIST>", recipeText);
            string[] hopsArray = hopsText.Split(new string[] { "<HOPS>" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string ht in hopsArray)
            {
                if (ht.Contains("</HOPS>"))
                {
                    Hops hops = new Hops();

                    // Read name
                    hops.Name = GetDataInbetween("<NAME>", ht);

                    // Read name
                    hops.Amount = Convert.ToDouble(GetDataInbetween("<AMOUNT>", ht));

                    // Read name
                    hops.BoilTime = TimeSpan.FromMinutes(Convert.ToDouble(GetDataInbetween("<BOILTIME>", ht)));

                    hopsList.Add(hops);
                }
            }

            #endregion

            BR.grainList = grains;
            BR.sessionInfo = si;
            BR.mashSteps = mashSteps;
            BR.spargeStep = ss;
            BR.hopsList = hopsList;
            return BR;
        }

        public static BreweryRecipe ImportRecipe()
        {
            BreweryRecipe BR = new BreweryRecipe();
            string recipeText;

            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Beersmith (*.bsmx)|*.bsmx";
            openDialog.FilterIndex = 1;

            try
            {
                if (openDialog.ShowDialog() == true)
                {
                    recipeText = File.ReadAllText(openDialog.FileName);
                }
                else
                {
                    BR = null;
                    return BR;
                }
            }
            catch
            {
                MessageBox.Show("Recipe file is corrupted and cannot be opened.", "Error");
                BR.sessionInfo.sessionName = "RecipeCorruptedError";
                return BR;
            }

            #region Read session info

            SessionInfo si = new SessionInfo();

            // Read name
            si.sessionName = GetDataInbetween("<F_R_NAME>", recipeText);

            // Read name
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";
            si.BatchSize = Math.Round(Convert.ToDouble(GetDataInbetween("<F_E_BATCH_VOL>", recipeText), provider) / 34.86875, 1);

            // Read name
            //si.BrewMethod = GetDataInbetween("<BREWMETHOD>", recipeText);

            // Read name
            BeerStyle bs = new BeerStyle();
            //bs.Name = GetDataInbetween("<STYLE>", recipeText);
            si.style = bs;

            #endregion

            #region Read grains

            ObservableCollection<Grain> grains = new ObservableCollection<Grain>();
            string grainsText = GetDataInbetween("<Data><Grain>",recipeText, "<Hops>");
            string[] grainsArray = grainsText.Split(new string[] { "<Grain>" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string grainText in grainsArray)
            {
                if (grainText.Contains("</F_G_NAME>"))
                {
                    MessageBox.Show(grainText);
                    Grain grain = new Grain();

                    // Read name
                    grain.name = GetDataInbetween("<F_G_NAME>", grainText);

                    // Read amount
                    MessageBox.Show(GetDataInbetween("<F_G_AMOUNT>", grainText));
                    grain.amount = Convert.ToDouble(GetDataInbetween("<F_G_AMOUNT>", grainText), provider);

                    grains.Add(grain);
                }
            }

            #endregion

            #region Read sparge

            SpargeStep ss = new SpargeStep();
            ss.spargeTemp = Convert.ToDouble(GetDataInbetween("<F_MH_SPARGE_TEMP>", recipeText), provider);

            #endregion

            #region Read hops

            ObservableCollection<Hops> hopsList = new ObservableCollection<Hops>();
            string[] hopsArray = recipeText.Split(new string[] { "<Hops>" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string ht in hopsArray)
            {
                if (ht.Contains("</Hops>"))
                {
                    Hops hops = new Hops();

                    // Read name
                    hops.Name = GetDataInbetween("<F_H_NAME>", ht);

                    // Read name
                    hops.Amount = Convert.ToDouble(GetDataInbetween("<F_H_AMOUNT>", ht),provider);

                    // Read name
                    hops.BoilTime = TimeSpan.FromMinutes(Convert.ToDouble(GetDataInbetween("<F_H_BOIL_TIME>", ht), provider));

                    hopsList.Add(hops);
                }
            }

            #endregion

            BR.grainList = grains;
            BR.sessionInfo = si;
            BR.spargeStep = ss;
            BR.hopsList = hopsList;

            return BR;
        }

        public static List<Hops> HopsFromDB()
        {
            List<Hops> listDB = new List<Hops>(SQLiteAcces.LoadHops());
            return listDB;
        }

        public static List<Grain> GrainsFromDB()
        {
            List<Grain> listDB = new List<Grain>(SQLiteAcces.LoadGrains());
            return listDB;
        }

        public static List<Yeast> YeastsFromDB()
        {
            return new List<Yeast>(SQLiteAcces.LoadYeasts());
        }

        public static List<BeerStyle> StylesFromDB()
        {
            return new List<BeerStyle>(SQLiteAcces.LoadStyles());
        }

        public static void HopsToDB(List<Hops> hopsList)
        {
            int newHops = 0;
            int totalHops = 0;

            foreach(Hops hops in hopsList)
            {
                totalHops++;

                if(SQLiteAcces.ItemExistsInDB("Hops", "Name", hops.Name) == false)
                {
                    newHops++;
                    SQLiteAcces.SaveHops(hops);
                }
            }

            MessageBox.Show((string.Format("{0} out of {1} hops were new and added to the database.", newHops, totalHops)),"Import finished");
        }

        public static void GrainsToDB(List<Grain> grainList)
        {
            int newGrains = 0;
            int totalGrains = 0;

            foreach(Grain grain in grainList)
            {
                totalGrains++;

                if (SQLiteAcces.ItemExistsInDB("Grains", "Name", grain.name) == false)
                {
                    newGrains++;
                    SQLiteAcces.SaveGrain(grain);
                }
            }

            MessageBox.Show((string.Format("{0} out of {1} grains were new and added to the database.", newGrains, totalGrains)), "Import finished");
        }

        public static void YeastToDB(List<Yeast> yeastList)
        {
            int newYeasts = 0;
            int totalYeasts = 0;

            foreach(Yeast yeast in yeastList)
            {
                totalYeasts++;

                if(SQLiteAcces.ItemExistsInDB("Yeasts", "Name", yeast.name) == false)
                {
                    newYeasts++;
                    SQLiteAcces.SaveYeast(yeast);
                }
            }

            MessageBox.Show((string.Format("{0} out of {1} yeasts were new and added to the database.", newYeasts, totalYeasts)), "Import finished");
        }

        public static void StylesToDB(List<BeerStyle> styleList)
        {
            int newStyles = 0;
            int totalStyles = 0;

            foreach (BeerStyle style in styleList)
            {
                totalStyles++;

                if (SQLiteAcces.ItemExistsInDB("Styles", "Name", style.Name) == false)
                {
                    newStyles++;
                    SQLiteAcces.SaveStyle(style);
                }
            }

            MessageBox.Show((string.Format("{0} out of {1} beer styles were new and added to the database.", newStyles, totalStyles)), "Import finished");
        }

        public static List<Hops> ImportHopsList(string hopsText)
        {
            List<Hops> hopList = new List<Hops>();
            string[] tempArray = hopsText.Split(new string[] { "<Hops>" }, StringSplitOptions.RemoveEmptyEntries);

            foreach(string _text in tempArray)
            {
                if (_text.Contains("<F_H_NAME>"))
                {
                    Hops hops = new Hops();

                    // Get name
                    hops.Name = GetDataInbetween("<F_H_NAME>", _text);

                    // Get origin
                    hops.Origin = GetDataInbetween("<F_H_ORIGIN>", _text);

                    // Get alpha
                    hops.Alpha = GetDataInbetween("<F_H_ALPHA>", _text);

                    // Get beta
                    hops.Beta = GetDataInbetween("<F_H_BETA>", _text);

                    // Get beta
                    hops.Notes = GetDataInbetween("<F_H_NOTES>", _text);

                    hopList.Add(hops);
                }
            }

            return hopList;
        }

        public static List<Grain> ImportGrainList(string grainText)
        {
            List<Grain> grainList = new List<Grain>();
            string[] tempArray = grainText.Split(new string[] { "<Grain>" }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach(string _text in tempArray)
            {
                if (_text.Contains("<F_G_NAME>"))
                {
                    Grain grain = new Grain();

                    // Get name
                    grain.name = GetDataInbetween("<F_G_NAME>", _text);

                    // Get origin
                    grain.origin = GetDataInbetween("<F_G_ORIGIN>", _text);

                    // Get beta
                    grain.notes = GetDataInbetween("<F_G_NOTES>", _text);

                    grainList.Add(grain);
                }
            }

            return grainList;
        }

        public static List<Yeast> ImportYeastList(string yeastText)
        {
            List<Yeast> yeastList = new List<Yeast>();

            string[] tempArray = yeastText.Split(new string[] { "<Yeast>" }, StringSplitOptions.RemoveEmptyEntries);
            string debugtxt = "";
            foreach(string _text in tempArray)
            {
                if (_text.Contains("<F_Y_NAME>"))
                {
                    Yeast yeast = new Yeast();

                    // Get name
                    yeast.name = GetDataInbetween("<F_Y_NAME>", _text);

                    // Get lab
                    yeast.lab = GetDataInbetween("<F_Y_LAB>", _text);

                    // Get best for
                    yeast.bestFor = GetDataInbetween("<F_Y_BEST_FOR>", _text);

                    //Get notes
                    yeast.notes = GetDataInbetween("<F_Y_NOTES>", _text);

                    yeastList.Add(yeast);
                }
            }

            return yeastList;
        }

        public static List<BeerStyle> ImportStyleList(string styleText)
        {
            List<BeerStyle> styleList = new List<BeerStyle>();
            string[] tempArray = styleText.Split(new string[] { "<Style>" }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach(string _text in tempArray)
            {
                if (_text.Contains("<F_S_NAME>"))
                {
                    BeerStyle style = new BeerStyle();

                    // Get Name
                    style.Name = GetDataInbetween("<F_S_NAME>", _text);

                    // Get Category
                    style.Category = GetDataInbetween("<F_S_CATEGORY>", _text);

                    // Get Description
                    style.Description = GetDataInbetween("<F_S_DESCRIPTION>", _text);

                    // Get Profile
                    style.Profile = GetDataInbetween("<F_S_PROFILE>", _text);

                    // Get Ingredients
                    style.Ingredients = GetDataInbetween("<F_S_INGREDIENTS>", _text);

                    styleList.Add(style);
                }
            }

            return styleList;
        }

        public static string GetDataInbetween(string startMarker, string recipeText, string endMarker = "")
        {
            // Check if end marker is empty. If it is, create it based on start marker
            if(endMarker.Length < 1)
            {
                endMarker = "</" + startMarker.Substring(1, startMarker.Length - 1);
            }

            string[] tempArray = recipeText.Split(new string[] { startMarker }, StringSplitOptions.RemoveEmptyEntries);
            string resultString = tempArray[1];
            tempArray = resultString.Split(new string[] { endMarker }, StringSplitOptions.RemoveEmptyEntries);
            resultString = tempArray[0];

            return resultString;
        }

        public static string AddProperty(string markerName, string content)
        {
            string result = "<" + markerName + ">" + content + "</" + markerName + ">\n";
            return result;
        }

        public static async void PlaySound(string fileName)
        {
            Uri uri = new Uri(AppDomain.CurrentDomain.BaseDirectory + fileName);
            SoundPlayer SP = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + fileName);
            SP.LoadCompleted += delegate (object sender, AsyncCompletedEventArgs e)
            {
                SP.Play();
            };
            SP.LoadAsync();
        }
    }
}
