using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApiOMDB.Data;
using WebApiOMDB.Entities;
using WebApiOMDB.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WebApiOMDB.Services
{
    public class MovieBackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly Random _random;
        private readonly string[] _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(c => c.ToString()).ToArray();
        private readonly ILogger<MovieBackgroundWorker> _logger;

        public MovieBackgroundWorker(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<MovieBackgroundWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _random = new Random();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MovieBackgroundWorker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                int intervalInSeconds = _configuration.GetValue<int>("MovieFetcher:IntervalSeconds");
                TimeSpan delay = TimeSpan.FromSeconds(intervalInSeconds);

                try
                {
                    string randomLetter = _alphabet[_random.Next(_alphabet.Length)];
                    string apiUrl = $"https://www.omdbapi.com/?apikey=7bdd2e4c&s={randomLetter}";

                    HttpResponseMessage response = await _httpClient.GetAsync(apiUrl, stoppingToken);
                    response.EnsureSuccessStatusCode();

                    var apiResponse = await response.Content.ReadAsStringAsync(stoppingToken);
                    var movieData = JsonSerializer.Deserialize<MovieApiResponse>(apiResponse);

                    if (movieData?.Search?.Any() == true)
                    {
                        var movie = movieData.Search.First();

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<OmdbDBContext>();

                            bool movieExists = await dbContext.Movies.AnyAsync(m => m.Title == movie.Title, stoppingToken);

                            if (!movieExists)
                            {
                                dbContext.Movies.Add(new Movie
                                {
                                    Title = movie.Title,
                                    Director = movie.Director,
                                    Genre = movie.Genre,
                                    Rating = movie.Rating,
                                    Description = movie.Description
                                });

                                await dbContext.SaveChangesAsync(stoppingToken);

                                _logger.LogInformation($"Added new movie to database: {movie.Title}");
                            }
                            else
                            {
                                _logger.LogInformation($"Movie already exists in database: {movie.Title}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in MovieBackgroundWorker");
                }

                await Task.Delay(delay, stoppingToken);
            }

            _logger.LogInformation("MovieBackgroundWorker is stopping.");
        }
    }
}
