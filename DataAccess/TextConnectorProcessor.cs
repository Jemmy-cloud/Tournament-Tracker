﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

// * Load the text file
// * convert the text to list <prizeModel>
// find the Max Id
// Add the new Record With the new Id (Max +1)
// Convert the prizes to list string
// Save the list<string> to the text file

namespace TrackerLibrary.DataAccess.TextHelpers
{

    public static class TextConnectorProcessor
    {
        public static string FullFilePath(this string fileName)
        {
            //C:\data\TournamentTracker\prizemodels.csv
            return $"{ ConfigurationManager.AppSettings["filepath"] }\\{ fileName }";
        }
        public static List<string> LoadFile(this string file)
        {
            if (!File.Exists(file))
            {
                return new List<string>() ;
            }
            return File.ReadAllLines(file).ToList();
  
        }
        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            List<PrizeModel> output = new List<PrizeModel>();  
            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                PrizeModel p = new PrizeModel();
                p.Id = int.Parse(cols[0]);
                p.PlaceNumber = int.Parse(cols[1]);
                p.PlaceName = cols[2];
                p.PrizeAmount = decimal.Parse(cols[3]);
                p.PricePercentage = double.Parse(cols[4]);
                output.Add(p);
            }
            return output;
        }

        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            List<PersonModel> output = new List<PersonModel>(); 
            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PersonModel p = new PersonModel();

                p.Id = int.Parse(cols[0]);
                p.FirstName = cols[1];
                p.LastName = cols[2];
                p.EmailAddress = cols[3];
                p.CellphoneNumber = cols[4];
                output.Add(p);  
                
            }
            return output;
        }

        public static List<TeamModel> ConvertToTeamModels(this List<string> lines, string peopleFileName)
        {
            List<TeamModel> output = new List<TeamModel>();
            List<PersonModel> people = peopleFileName.FullFilePath().LoadFile().ConvertToPersonModels();
            foreach (String Line in lines)
            {
                String [] cols = Line.Split(',');
                TeamModel t = new TeamModel();  
                t.Id = int.Parse(cols[0]);
                t.TeamName = cols[1];

                string[] personIds = cols[2].Split('|');   

                foreach (String id in personIds)
                {
                    t.TeamMembers.Add(people.Where(x => x.Id == int.Parse(id)).First());
                }
            }
            return output;

        }




        public static void SaveToPrizeFile(this List<PrizeModel> models, string FileName)
        {
            List <string> lines = new List<string>();
            foreach (PrizeModel p in models)
            {
                lines.Add($"{p.Id},{p.PlaceNumber},{p.PlaceName},{p.PrizeAmount},{p.PricePercentage}");
            }
            File.WriteAllLines(FileName.FullFilePath(), lines);
        }

        public static void SaveToPeopleFile(this List<PersonModel> models, string FileName)
        {
            List <string> lines = new List<string>();
            foreach (PersonModel p in models)
            {
                lines.Add($"{p.Id},{p.FirstName},{p.LastName},{p.EmailAddress},{p.CellphoneNumber}");
            }
            File.WriteAllLines(FileName.FullFilePath(), lines);
        }

        public static void SaveToTeamFile(this List<TeamModel> models, string FileName)
        {
            List<string> lines = new List<string>();
            foreach (TeamModel t in models)
            {
                lines.Add($" {t.Id} , {t.TeamName} , {ConvertPeopleListToString(t.TeamMembers)}");
            }

            File.WriteAllLines(FileName.FullFilePath(), lines);
        }

            private static string ConvertPeopleListToString(List<PersonModel> people)
        {
            string output = ",";

            if (people.Count == 0)
            {
                return "";
            }
            foreach (PersonModel p in people)
            {
                output +=  $"{p.Id}|";
            }
            output = output.Substring(0, output.Length - 1);
            return output;
        }
        
    }


}
