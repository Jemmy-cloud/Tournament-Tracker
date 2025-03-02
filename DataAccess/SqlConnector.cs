﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;


//@PlaceNumber int,
//@PlaceName nvarchar(50),
//@PrizeAmount money,
//@PricePercentage float,
//@id int = 0 output



namespace TrackerLibrary.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        private const string db = "Tournaments";
        
        
        public PersonModel CreatePerson(PersonModel model)
        {
          using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var p = new DynamicParameters();
                p.Add("@FirstName", model.FirstName);
                p.Add("@LastName", model.LastName);
                p.Add("@EmailAddress" , model.EmailAddress);    
                p.Add("@CellphoneNumber" , model.CellphoneNumber);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPeople_Insert", p, commandType: CommandType.StoredProcedure); 

                model.Id = p.Get<int>("@id");

                return model;
            }
        }

        // TODO - MAKE THE CREATE PRIZE METHOD ACTUALLY SAVES TO THE DATABASE 
        /// <summary>
        /// Saves new Prize in database
        /// </summary>
        /// <param name="model"> the price information</param>
        /// <returns> returns the prize information including the unique identifier</returns>


        public PrizeModel CreatePrize(PrizeModel model)
        {

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var p = new DynamicParameters();
                p.Add("@PlaceNumber" , model.PlaceNumber);
                p.Add("@placeName", model.PlaceName);
                p.Add("@PrizeAmount" , model.PrizeAmount);
                p.Add("@PricePercentage", model.PricePercentage);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPrizes_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                return model;
            }
        }

        public TeamModel CreateTeam(TeamModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                var p = new DynamicParameters();
                p.Add("@TeamName", model.TeamName);
             
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTeams_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                foreach ( PersonModel tm in model.TeamMembers )
                {
                    p = new DynamicParameters();
                    p.Add("@TeamId", model.Id);
                    p.Add("@PersonId", tm.Id);
                    
                   

                    connection.Execute("dbo.spTeamMembers_Insert", p, commandType: CommandType.StoredProcedure);

                }
                return model;
            }
        }

        public TournamentModel CreateTournament(TournamentModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                SaveTournament(connection, model);
                foreach (PrizeModel pz in model.Prizes)
                {
                    p = new DynamicParameters();
                    p.Add("@tournamentId", model.Id);
                    p.Add("@PrizeId", pz.Id);
                    p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    foreach (TeamModel tm in model.EnteredTeams)
                    {
                        p = new DynamicParameters();
                        p.Add("@tournamentId", model.Id);
                        p.Add("@TeamId", tm.Id);
                        p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);
                    }

                    connection.Execute("dbo.spTournamentEntries_Insert", p, commandType: CommandType.StoredProcedure);

                }
                return model;
            }
        }


            private void SaveTournament(IDbConnection connection, TournamentModel model)
            {



                var p = new DynamicParameters();
                p.Add("@TournamentName", model.TournamentName);
                p.Add("@EntryFee", model.EntryFee);

                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentPrizes_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");
            }

            public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();

            }

            return output;
        }

        public List<TeamModel> GetTeam_All()
        {
            List<TeamModel> output;
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
            {

                output = connection.Query<TeamModel>("dbo.spTeam_GetAll").ToList();
                foreach (TeamModel team in output)
                {
                    var p = new DynamicParameters();
                    p.Add("@TeamId", team.Id); 
                    team.TeamMembers = connection.Query<PersonModel>("spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }

            return output;
        }
}
}