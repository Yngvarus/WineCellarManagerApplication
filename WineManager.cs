﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WineCellarManager
{
    public class WineManager
    {
        private List<WineBottle> wineBottles = new List<WineBottle>();
        private String conString = "aggiungere la stringa di connessione";

        public WineManager()
        {
            LoadWineBottlesFromDatabase();
        }

        private void LoadWineBottlesFromDatabase()
        {
            using (SqlConnection conn = new SqlConnection(conString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT * FROM WineBottles";
                    SqlCommand command = new SqlCommand(query, conn);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WineBottle wineBottle = new WineBottle(
                                reader["Name"].ToString(),
                                reader["Vineyard"].ToString(),
                                reader["Location"].ToString(),
                                Convert.ToInt32(reader["Year"]),
                                reader["Style"].ToString(),
                                reader["CellarLocation"].ToString(),
                                Convert.ToInt32(reader["Stock"]),
                                double.Parse(reader["SellingPrice"].ToString()),
                                double.Parse(reader["BuyingPrice"].ToString()),
                                reader["TastingNotes"].ToString()
                            );

                            wineBottles.Add(wineBottle);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errore durante il caricamento del DB: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public List<WineBottle> GetWineBottles()
        {
            return wineBottles;
        }

        public void AddWineBottle(WineBottle bottle)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conString))
                {
                    conn.Open();

                    string query = @"INSERT INTO WineBottles (Name, Vineyard, Location, Year, Style, CellarLocation, Stock, SellingPrice, BuyingPrice, TastingNotes)
                                    VALUES (@Name, @Vineyard, @Location, @Year, @Style, @CellarLocation, @Stock, @SellingPrice, @BuyingPrice, @TastingNotes)";

                    SqlCommand command = new SqlCommand(query, conn);
                    command.Parameters.AddWithValue("@Name", bottle.Name);
                    command.Parameters.AddWithValue("@Vineyard", bottle.Vineyard);
                    command.Parameters.AddWithValue("@Location", bottle.Location);
                    command.Parameters.AddWithValue("@Year", bottle.Year);
                    command.Parameters.AddWithValue("@Style", bottle.Style);
                    command.Parameters.AddWithValue("@CellarLocation", bottle.CellarLocation);
                    command.Parameters.AddWithValue("@Stock", bottle.Stock);
                    command.Parameters.AddWithValue("@SellingPrice", bottle.SellingPrice);
                    command.Parameters.AddWithValue("@BuyingPrice", bottle.BuyingPrice);
                    command.Parameters.AddWithValue("@TastingNotes", bottle.TastingNotes);

                    command.ExecuteNonQuery();
                }
                // Aggiorna la lista interna dopo l'aggiunta di una nuova bottiglia
                wineBottles.Add(bottle);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore durante l'aggiunta della bottiglia: " + ex.Message);
            }
        }

        public void RemoveWineBottle(WineBottle bottle)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conString))
                {
                    conn.Open();

                    string query = @"DELETE FROM WineBottles WHERE Name = @bottleName AND Year = @bottleYear";

                    SqlCommand command = new SqlCommand(query, conn);
                    command.Parameters.AddWithValue("@bottleName", bottle.Name);
                    command.Parameters.AddWithValue("@bottleYear", bottle.Year);

                    command.ExecuteNonQuery();
                }
                // Rimuovi la bottiglia dalla lista interna dopo la rimozione dal database
                wineBottles.Remove(bottle);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore durante la rimozione della bottiglia: " + ex.Message);
            }
        }

        // Metodo per aggiornare un singolo attributo di una bottiglia di vino
        public void UpdateWineBottleAttribute(WineBottle bottle, String attributeName, object newValue)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conString))
                {
                    conn.Open();

                    string query = $@"UPDATE WineBottles SET {attributeName} = @newValue WHERE Name = @bottleName AND Year = @bottleYear";

                    SqlCommand command = new SqlCommand(query, conn);
                    command.Parameters.AddWithValue("@newValue", newValue);
                    command.Parameters.AddWithValue("@bottleName", bottle.Name);
                    command.Parameters.AddWithValue("@bottleYear", bottle.Year);

                    command.ExecuteNonQuery();
                }
                // Aggiorna la lista interna dopo la modifica dell'attributo
                int index = wineBottles.FindIndex(b => b.Name == bottle.Name && b.Year == bottle.Year);
                if (index != -1)
                {
                    switch (attributeName)
                    {
                        case "Name":
                            wineBottles[index].Name = (string)newValue;
                            break;
                        case "Vineyard":
                            wineBottles[index].Vineyard = (string)newValue;
                            break;
                        case "Location":
                            wineBottles[index].Location = (string)newValue;
                            break;
                        case "Year":
                            wineBottles[index].Year = (int)newValue;
                            break;
                        case "Style":
                            wineBottles[index].Style = (string)newValue;
                            break;
                        case "CellarLocation":
                            wineBottles[index].CellarLocation = (string)newValue;
                            break;
                        case "Stock":
                            wineBottles[index].Stock = (int)newValue;
                            break;
                        case "SellingPrice":
                            wineBottles[index].SellingPrice = (double)newValue;
                            break;
                        case "BuyingPrice":
                            wineBottles[index].BuyingPrice = (double)newValue;
                            break;
                        case "TastingNotes":
                            wineBottles[index].TastingNotes = (string)newValue;
                            break;
                        default:
                            Console.WriteLine("Attributo non valido");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore durante l'aggiornamento dell'attributo: " + ex.Message);
            }
        }

        // Metodo per controllare il numero di bottiglie in magazzino e rimuovere se minore di 1
        public void CheckStockAndRemoveIfNeeded()
        {
            foreach (var bottle in wineBottles)
            {
                if (bottle.Stock < 1)
                {
                    RemoveWineBottle(bottle);
                    Console.WriteLine($"La bottiglia {bottle.Name} {bottle.Year} è stata rimossa perché il numero in magazzino era inferiore a 1.");
                }
            }
        }
    }
}
