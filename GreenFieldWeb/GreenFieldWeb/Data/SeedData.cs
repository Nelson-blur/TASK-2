using GreenFieldWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GreenFieldWeb.Data
{
    public class SeedData
    {
        public static async Task SeedUsersAndRoles(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {//Seeded Roles
            string[] roleNames = { "Admin", "Producer", "Standard", "Developer" };
            foreach (string roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var role = new IdentityRole(roleName);
                    await roleManager.CreateAsync(role);
                }

            }
            //Seeding users and assigning roles, one for each typr of user
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = "admin@example.com", Email = "admin@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "Password123!");

            }
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            var producerUser = await userManager.FindByEmailAsync("producer@example.com");
            if (producerUser == null)
            {
                producerUser = new IdentityUser { UserName = "producer@example.com", Email = "producer@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(producerUser, "Password123!");

            }
            if (!await userManager.IsInRoleAsync(producerUser, "Producer"))
            {
                await userManager.AddToRoleAsync(producerUser, "Producer");
            }
            var producerUser2 = await userManager.FindByEmailAsync("producer2@example.com");
            if (producerUser2 == null)
            {
                producerUser2 = new IdentityUser { UserName = "producer2@example.com", Email = "producer2@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(producerUser2, "Password123!");
            }
            if (!await userManager.IsInRoleAsync(producerUser2, "Producer"))
            {
                await userManager.AddToRoleAsync(producerUser2, "Producer");
            }
            var producerUser3 = await userManager.FindByEmailAsync("producer3@example.com");
            if (producerUser3 == null)
            {
                producerUser3 = new IdentityUser { UserName = "producer3@example.com", Email = "producer3@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(producerUser3, "Password123!");
            }
            if (!await userManager.IsInRoleAsync(producerUser3, "Producer"))
            {
                await userManager.AddToRoleAsync(producerUser3, "Producer");
            }
            var devUser = await userManager.FindByEmailAsync("dev@example.com");
            if (devUser == null)
            {
                devUser = new IdentityUser { UserName = "dev@example.com", Email = "dev@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(devUser, "Password123!");
            }
            if (!await userManager.IsInRoleAsync(devUser, "Developer"))
            {
                await userManager.AddToRoleAsync(devUser, "Developer");
            }
            var normalUser = await userManager.FindByEmailAsync("user@example.com");
            if (normalUser == null)
            {
                normalUser = new IdentityUser { UserName = "user@example.com", Email = "user@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(normalUser, "Password123!");
            }
            if (!await userManager.IsInRoleAsync(normalUser, "Standard"))
            {
                await userManager.AddToRoleAsync(normalUser, "Standard");
            }


        }

        public static async Task SeedProducers(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            //Finds User by Email
            var ProducerUser1 = await userManager.FindByEmailAsync("producer@example.com");
            var ProducerUser2 = await userManager.FindByEmailAsync("producer2@example.com");
            var ProducerUser3 = await userManager.FindByEmailAsync("producer3@example.com");

            if (ProducerUser1 == null || ProducerUser2 == null || ProducerUser3 == null)
            {
                throw new Exception("Producer users not found. Ensure they are seeded before running this method.");
            }
            if (context.Producers.Any())
                return;

            var producers = new List<Producers>
            {
                new Producers
                {
                        ProducerName = "Fresh Fields",
                        Description = "Fresh Fields is a farm that has been providing high-quality produce for over 20 years. ",
                        ContactEmail = "contact@freshfields.co.uk",
                        BusinessLocation = "Costwolds",
                        UserId = ProducerUser1.Id
                },
                new Producers
                {
                        ProducerName = "Green Acres",
                        Description = "Green Acres is a family-owned farm that specializes in organic produce. ",
                        ContactEmail = "contact@greenacres.co.uk",
                        BusinessLocation = "Yorkshire",
                        UserId = ProducerUser2.Id
                },
                new Producers
                {
                        ProducerName = "Sunny Farms",
                        Description = "Sunny Farms is a farm that focuses on sustainable farming practices and offers a wide variety of fruits and vegetables. ",
                        ContactEmail = "contact@sunnyfarms.co.uk",
                        BusinessLocation = "Devon",
                        UserId = ProducerUser3.Id
                }
            };
            context.Producers.AddRange(producers);
            await context.SaveChangesAsync();
        }

        public static async Task SeedProducts(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var FreshFields = await context.Producers.FirstOrDefaultAsync(s => s.ProducerName == "Fresh Fields");
            var GreenAcres = await context.Producers.FirstOrDefaultAsync(s => s.ProducerName == "Green Acres");
            var SunnyFarms = await context.Producers.FirstOrDefaultAsync(s => s.ProducerName == "Sunny Farms");
            if (FreshFields == null || GreenAcres == null || SunnyFarms == null)
            {
                throw new Exception("Producers not found. Ensure they are seeded before running this method.");
            }

            if (context.Products.Any())
                return;
            var products = new List<Products>
            {
                new Products
                {
                    ProductName = "Organic Honey",

                    Price = 1.88f,
                    Stock = 50,
                    Description = "Pure organic honey collected from wildflowers.",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 10),
                    IsAvailable = true,
                    AllergenInformation = "None",
                    FarmingMethod = "Organic",
                    ProducersId = FreshFields.ProducersId,
                    ImageUrl = "/images/organic-honey.jpg"


                },
                new Products
                {
                    ProductName = "Free-range Eggs",
                    Price = 5.99f,
                    Stock = 200,
                    Description = "Eggs from free-range chickens.",
                    CreatedAt = new DateTime(2025, 3, 1),
                    UpdatedAt = new DateTime(2025, 3, 3),
                    IsAvailable = true,
                    AllergenInformation = "Contains Eggs",
                    FarmingMethod = "Free-range",
                    ProducersId = GreenAcres.ProducersId,
                    ImageUrl = "/images/free-range-eggs.jpg"
                },
                new Products
                {

                    ProductName = "Fresh Milk",
                    Price = 3.49f,
                    Stock = 100,
                    Description = "Locally sourced fresh cow milk.",
                    CreatedAt = new DateTime(2026, 2, 1),
                    UpdatedAt = new DateTime(2026, 2, 5),
                    IsAvailable = true,
                    AllergenInformation = "Contains Dairy",
                    FarmingMethod = "Conventional",
                    ProducersId = SunnyFarms.ProducersId,
                    ImageUrl = "/images/fresh-milk.jpg"
                },
                new Products
                {
                    ProductName = "Organic Carrots",
                    Price = 0.99f,
                    Stock = 150,
                    Description = "Crisp and sweet organic carrots.",
                    CreatedAt = new DateTime(2025, 4, 1),
                    UpdatedAt = new DateTime(2025, 4, 10),
                    IsAvailable = true,
                    AllergenInformation = "None",
                    FarmingMethod = "Organic",
                    ProducersId = FreshFields.ProducersId,
                    ImageUrl = "/images/organic-carrots.jpg"
                },
                new Products
                {
                    ProductName = "Free-range Chicken",
                    Price = 8.99f,
                    Stock = 80,
                    Description = "Juicy and tender free-range chicken.",
                    CreatedAt = new DateTime(2025, 5, 1),
                    UpdatedAt = new DateTime(2025, 5, 15),
                    IsAvailable = true,
                    AllergenInformation = "Contains Poultry",
                    FarmingMethod = "Free-range",
                    ProducersId = GreenAcres.ProducersId,
                    ImageUrl = "/images/free-range-chicken.jpg"
                },
                new Products
                {
                    ProductName = "Fresh Strawberries",
                    Price = 2.99f,
                    Stock = 120,
                    Description = "Sweet and juicy fresh strawberries.",
                    CreatedAt = new DateTime(2026, 3, 1),
                    UpdatedAt = new DateTime(2026, 3, 10),
                    IsAvailable = true,
                    AllergenInformation = "None",
                    FarmingMethod = "Conventional",
                    ProducersId = SunnyFarms.ProducersId,
                    ImageUrl = "/images/fresh-strawberries.jpg"

                },
                new Products
                {
                    ProductName = "Organic Lettuce",
                    Price = 1.49f,
                    Stock = 100,
                    Description = "Crisp and fresh organic lettuce.",
                    CreatedAt = new DateTime(2025, 6, 1),
                    UpdatedAt = new DateTime(2025, 6, 10),
                    IsAvailable = true,
                    AllergenInformation = "None",
                    FarmingMethod = "Organic",
                    ProducersId = FreshFields.ProducersId,
                    ImageUrl = "/images/organic-lettuce.jpg"
                },
                new Products
                {
                    ProductName = "Free-range Pork",
                    Price = 9.99f,
                    Stock = 60,
                    Description = "Tender and flavorful free-range pork.",
                    CreatedAt = new DateTime(2025, 7, 1),
                    UpdatedAt = new DateTime(2025, 7, 15),
                    IsAvailable = true,
                    AllergenInformation = "Contains Pork",
                    FarmingMethod = "Free-range",
                    ProducersId = GreenAcres.ProducersId,
                    ImageUrl = "/images/free-range-pork.jpg"
                },
                new Products
                {
                    ProductName = "Fresh Blueberries",
                    Price = 3.49f,
                    Stock = 90,
                    Description = "Sweet and plump fresh blueberries.",
                    CreatedAt = new DateTime(2026, 4, 1),
                    UpdatedAt = new DateTime(2026, 4, 10),
                    IsAvailable = true,
                    AllergenInformation = "None",
                    FarmingMethod = "Conventional",
                    ProducersId = SunnyFarms.ProducersId,
                    ImageUrl = "/images/fresh-blueberries.jpg"
                },
                new Products
                {
                    ProductName = "Organic Potatoes",
                    Price = 0.79f,
                    Stock = 200,
                    Description = "Starchy and delicious organic potatoes.",
                    CreatedAt = new DateTime(2025, 8, 1),
                    UpdatedAt = new DateTime(2025, 8, 10),
                    IsAvailable = true,
                    AllergenInformation = "None",
                    FarmingMethod = "Organic",
                    ProducersId = FreshFields.ProducersId,
                    ImageUrl = "/images/organic-potatoes.jpg"
                }

            };
            context.Products.AddRange(products);
            await context.SaveChangesAsync();

        }




    }
}