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

        }
    }
}