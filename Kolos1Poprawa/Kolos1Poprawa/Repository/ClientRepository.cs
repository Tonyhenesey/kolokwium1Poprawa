using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using Kolos1Poprawa.Models;
using System.Collections.Generic;

namespace Kolos1Poprawa.Repository
{
    public class ClientRepository : IClientRepository
    {
        private readonly string _connectionString;

        public ClientRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }

        public async Task<ClientDTO> GetClientDataById(int clientId)
        {
            var client = new ClientDTO();
            var rentals = new List<Rentals>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string clientQuery = @"SELECT ID, FirstName, LastName, Address FROM clients WHERE ID = @clientId";
                using (SqlCommand clientCommand = new SqlCommand(clientQuery, connection))
                {
                    clientCommand.Parameters.AddWithValue("@clientId", clientId);

                    using (SqlDataReader clientReader = await clientCommand.ExecuteReaderAsync())
                    {
                        if (await clientReader.ReadAsync())
                        {
                            client.id = clientReader.GetInt32("ID");
                            client.firstName = clientReader.GetString("FirstName");
                            client.lastName = clientReader.GetString("LastName");
                            client.adress = clientReader.GetString("Address");
                        }
                    }
                }

                string rentalsQuery =
                    @"SELECT c.vin, c.ColorID, c.ModelID, cr.DateFrom, cr.DateTo, cr.TotalPrice, co.Name as Color, m.Name as Model
                      FROM car_rentals cr
                      JOIN cars c ON cr.CarID = c.ID
                      JOIN colors co ON c.ColorID = co.ID
                      JOIN models m ON c.ModelID = m.ID
                      WHERE cr.ClientID = @clientId";
                using (SqlCommand rentalsCommand = new SqlCommand(rentalsQuery, connection))
                {
                    rentalsCommand.Parameters.AddWithValue("@clientId", clientId);

                    using (SqlDataReader rentalsReader = await rentalsCommand.ExecuteReaderAsync())
                    {
                        while (await rentalsReader.ReadAsync())
                        {
                            var rental = new Rentals
                            {
                                vin = rentalsReader.GetString("vin"),
                                color = rentalsReader.GetString("Color"),
                                model = rentalsReader.GetString("Model"),
                                dateFrom = rentalsReader.GetDateTime("DateFrom"),
                                dateTo = rentalsReader.GetDateTime("DateTo"),
                                totalPrice = rentalsReader.GetInt32("TotalPrice")
                            };
                            rentals.Add(rental);
                        }
                    }
                }
            }

            client.Rentals = rentals;
            return client;
        }

        public async Task<PostClientDTO> AddClientAndRental(PostClientDTO postClientDTO)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertClientQuery = @"INSERT INTO clients (FirstName, LastName, Address) 
                                                     OUTPUT INSERTED.ID
                                                     VALUES (@FirstName, @LastName, @Address)";
                        int clientId;
                        using (SqlCommand cmd = new SqlCommand(insertClientQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@FirstName", postClientDTO.client.firstName);
                            cmd.Parameters.AddWithValue("@LastName", postClientDTO.client.lastName);
                            cmd.Parameters.AddWithValue("@Address", postClientDTO.client.adress);

                            clientId = (int)await cmd.ExecuteScalarAsync();
                        }

                        string checkCarQuery = "SELECT COUNT(1) FROM cars WHERE ID = @CarID";
                        using (SqlCommand checkCarCmd = new SqlCommand(checkCarQuery, connection, transaction))
                        {
                            checkCarCmd.Parameters.AddWithValue("@CarID", postClientDTO.carId);
                            int carExists = (int)await checkCarCmd.ExecuteScalarAsync();
                            if (carExists == 0)
                            {
                                transaction.Rollback();
                                return null;
                            }
                        }

                        string priceQuery = "SELECT PricePerDay FROM cars WHERE ID = @CarID";
                        double pricePerDay;
                        using (SqlCommand priceCmd = new SqlCommand(priceQuery, connection, transaction))
                        {
                            priceCmd.Parameters.AddWithValue("@CarID", postClientDTO.carId);
                            pricePerDay = (int)await priceCmd.ExecuteScalarAsync();
                        }
                        double totalPrice = (postClientDTO.dateTo - postClientDTO.dateFrom).TotalDays * pricePerDay;

                        string insertRentalQuery = @"INSERT INTO car_rentals (ClientID, CarID, DateFrom, DateTo, TotalPrice)
                                                     VALUES (@ClientID, @CarID, @DateFrom, @DateTo, @TotalPrice)";
                        using (SqlCommand rentalCmd = new SqlCommand(insertRentalQuery, connection, transaction))
                        {
                            rentalCmd.Parameters.AddWithValue("@ClientID", clientId);
                            rentalCmd.Parameters.AddWithValue("@CarID", postClientDTO.carId);
                            rentalCmd.Parameters.AddWithValue("@DateFrom", postClientDTO.dateFrom);
                            rentalCmd.Parameters.AddWithValue("@DateTo", postClientDTO.dateTo);
                            rentalCmd.Parameters.AddWithValue("@TotalPrice", totalPrice);

                            await rentalCmd.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                        return postClientDTO;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception(" error adding the client and rental", ex);
                    }
                }
            }
        }
    }
}
