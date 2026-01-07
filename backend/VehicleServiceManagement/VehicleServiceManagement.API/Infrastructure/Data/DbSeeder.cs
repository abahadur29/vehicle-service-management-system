using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;

namespace VehicleServiceManagement.API.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "Manager", "Technician", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }


            // ========== ADMIN USERS ==========
            var admin1 = await CreateUserWithRole(userManager, "System Administrator", "admin@vehicle.com", "Admin@123", "Admin");

            // ========== MANAGER USERS ==========
            var manager1 = await CreateUserWithRole(userManager, "Ramesh Kumar", "manager@vehicle.com", "Manager@123", "Manager");
            var manager2 = await CreateUserWithRole(userManager, "Sunita Patel", "sunita.manager@vehicle.com", "Manager@123", "Manager");
            var manager3 = await CreateUserWithRole(userManager, "Amit Singh", "amit.manager@vehicle.com", "Manager@123", "Manager");

            // ========== TECHNICIAN USERS ==========
            var tech1 = await CreateUserWithRole(userManager, "Rajiv Pratap", "tech@vehicle.com", "Tech@123", "Technician");
            var tech2 = await CreateUserWithRole(userManager, "Priya Sharma", "priya.tech@vehicle.com", "Tech@123", "Technician");
            var tech3 = await CreateUserWithRole(userManager, "Anita Desai", "anita.tech@vehicle.com", "Tech@123", "Technician");
            var tech4 = await CreateUserWithRole(userManager, "Vikram Mehta", "vikram.tech@vehicle.com", "Tech@123", "Technician");
            var tech5 = await CreateUserWithRole(userManager, "Kavita Reddy", "kavita.tech@vehicle.com", "Tech@123", "Technician");
            var tech6 = await CreateUserWithRole(userManager, "Arjun Nair", "arjun.tech@vehicle.com", "Tech@123", "Technician");

            // ========== CUSTOMER USERS ==========
            var customer1 = await CreateUserWithRole(userManager, "Rahul Verma", "customer@vehicle.com", "Customer@123", "Customer");
            var customer2 = await CreateUserWithRole(userManager, "Sneha Gupta", "sneha.customer@vehicle.com", "Customer@123", "Customer");
            var customer3 = await CreateUserWithRole(userManager, "Mohit Joshi", "mohit.customer@vehicle.com", "Customer@123", "Customer");
            var customer4 = await CreateUserWithRole(userManager, "Pooja Shah", "pooja.customer@vehicle.com", "Customer@123", "Customer");
            var customer5 = await CreateUserWithRole(userManager, "Rohit Agarwal", "rohit.customer@vehicle.com", "Customer@123", "Customer");
            var customer6 = await CreateUserWithRole(userManager, "Neha Kapoor", "neha.customer@vehicle.com", "Customer@123", "Customer");
            var customer7 = await CreateUserWithRole(userManager, "Suresh Iyer", "suresh.customer@vehicle.com", "Customer@123", "Customer");
            var customer8 = await CreateUserWithRole(userManager, "Divya Menon", "divya.customer@vehicle.com", "Customer@123", "Customer");
            var customer9 = await CreateUserWithRole(userManager, "Aman Khanna", "aman.customer@vehicle.com", "Customer@123", "Customer");
            var customer10 = await CreateUserWithRole(userManager, "Kiran Rao", "kiran.customer@vehicle.com", "Customer@123", "Customer");

            // ========== PARTS INVENTORY ==========
            if (!context.Parts.Any())
            {
                var partsList = new List<Part>
                {
                    new Part { Name = "Synthetic Engine Oil", UnitPrice = 1500.00m, StockQuantity = 50 },
                    new Part { Name = "Oil Filter (Premium)", UnitPrice = 550.00m, StockQuantity = 30 },
                    new Part { Name = "Front Brake Pads", UnitPrice = 2800.00m, StockQuantity = 12 },
                    new Part { Name = "Rear Brake Pads", UnitPrice = 2400.00m, StockQuantity = 4 },
                    new Part { Name = "Air Filter", UnitPrice = 800.00m, StockQuantity = 25 },
                    new Part { Name = "Spark Plug (Iridium)", UnitPrice = 450.00m, StockQuantity = 3 },
                    new Part { Name = "Wiper Blades", UnitPrice = 1200.00m, StockQuantity = 15 },
                    new Part { Name = "Headlight Bulb (LED)", UnitPrice = 3500.00m, StockQuantity = 8 },
                    new Part { Name = "Coolant (5 Liters)", UnitPrice = 1800.00m, StockQuantity = 20 },
                    new Part { Name = "Brake Fluid", UnitPrice = 950.00m, StockQuantity = 10 },
                    new Part { Name = "Battery (65AH)", UnitPrice = 8500.00m, StockQuantity = 5 },
                    new Part { Name = "Timing Belt", UnitPrice = 4500.00m, StockQuantity = 7 },
                    new Part { Name = "Fan Belt", UnitPrice = 1200.00m, StockQuantity = 14 },
                    new Part { Name = "Clutch Plate", UnitPrice = 12500.00m, StockQuantity = 2 },
                    new Part { Name = "Shock Absorber (Front)", UnitPrice = 6500.00m, StockQuantity = 6 },
                    new Part { Name = "Shock Absorber (Rear)", UnitPrice = 5800.00m, StockQuantity = 6 },
                    new Part { Name = "Radiator Cap", UnitPrice = 350.00m, StockQuantity = 40 },
                    new Part { Name = "Thermostat Valve", UnitPrice = 1100.00m, StockQuantity = 10 },
                    new Part { Name = "Wheel Bearing", UnitPrice = 3200.00m, StockQuantity = 8 },
                    new Part { Name = "Transmission Fluid", UnitPrice = 2200.00m, StockQuantity = 18 },
                    new Part { Name = "Cabin Air Filter", UnitPrice = 750.00m, StockQuantity = 22 },
                    new Part { Name = "Fuel Filter", UnitPrice = 1400.00m, StockQuantity = 11 },
                    new Part { Name = "Ignition Coil", UnitPrice = 3800.00m, StockQuantity = 9 },
                    new Part { Name = "Alternator Belt", UnitPrice = 900.00m, StockQuantity = 13 },
                    new Part { Name = "Power Steering Fluid", UnitPrice = 1100.00m, StockQuantity = 15 },
                    new Part { Name = "Exhaust Gasket", UnitPrice = 600.00m, StockQuantity = 20 },
                    new Part { Name = "Water Pump", UnitPrice = 5500.00m, StockQuantity = 4 },
                    new Part { Name = "Door Handle (Inner)", UnitPrice = 850.00m, StockQuantity = 5 },
                    new Part { Name = "Side Mirror Glass", UnitPrice = 1500.00m, StockQuantity = 7 },
                    new Part { Name = "Washer Fluid (Concentrate)", UnitPrice = 250.00m, StockQuantity = 50 }
                };
                context.Parts.AddRange(partsList);
                await context.SaveChangesAsync();
            }

            // ========== SERVICE CATEGORIES ==========
            if (!context.ServiceCategories.Any())
            {
                var now = DateTime.UtcNow;
                var categoriesList = new List<ServiceCategory>
                {
                    new ServiceCategory { Name = "Express Wash", BasePrice = 400m, Description = "Quick exterior cleaning.", CreatedAt = now },
                    new ServiceCategory { Name = "Deluxe Interior Detail", BasePrice = 1500m, Description = "Deep steam cleaning of seats and mats.", CreatedAt = now },
                    new ServiceCategory { Name = "Standard Oil Change", BasePrice = 1200m, Description = "Drain and refill engine oil with filter change.", CreatedAt = now },
                    new ServiceCategory { Name = "Full Preventive Maintenance", BasePrice = 5000m, Description = "60-point inspection and fluid top-ups.", CreatedAt = now },
                    new ServiceCategory { Name = "Brake System Overhaul", BasePrice = 3500m, Description = "Full brake inspection and pad replacement.", CreatedAt = now },
                    new ServiceCategory { Name = "Wheel Alignment", BasePrice = 800m, Description = "Precision 4-wheel laser alignment.", CreatedAt = now },
                    new ServiceCategory { Name = "Wheel Balancing", BasePrice = 600m, Description = "Electronic balancing for all four tires.", CreatedAt = now },
                    new ServiceCategory { Name = "AC Gas Refill", BasePrice = 2200m, Description = "Refrigerant recharge and leak check.", CreatedAt = now },
                    new ServiceCategory { Name = "Engine Diagnostic Scan", BasePrice = 1000m, Description = "Full OBD-II computer error code scanning.", CreatedAt = now },
                    new ServiceCategory { Name = "Suspension Tuning", BasePrice = 4500m, Description = "Inspection of struts and bushings.", CreatedAt = now },
                    new ServiceCategory { Name = "Transmission Flush", BasePrice = 3800m, Description = "Complete automatic transmission fluid exchange.", CreatedAt = now },
                    new ServiceCategory { Name = "Battery Health Check", BasePrice = 300m, Description = "Voltage and load test.", CreatedAt = now },
                    new ServiceCategory { Name = "Headlight Restoration", BasePrice = 1200m, Description = "Polishing hazy headlight lenses.", CreatedAt = now },
                    new ServiceCategory { Name = "Radiator Flush", BasePrice = 1800m, Description = "Cooling system cleaning and fresh antifreeze.", CreatedAt = now },
                    new ServiceCategory { Name = "Clutch Adjustment", BasePrice = 1500m, Description = "Manual transmission clutch pedal calibration.", CreatedAt = now },
                    new ServiceCategory { Name = "Fuel Injector Cleaning", BasePrice = 2500m, Description = "Pressurized cleaning of fuel nozzles.", CreatedAt = now },
                    new ServiceCategory { Name = "Tire Rotation", BasePrice = 400m, Description = "Swapping tires to ensure even wear.", CreatedAt = now },
                    new ServiceCategory { Name = "Ceramic Coating", BasePrice = 15000m, Description = "High-end paint protection layer.", CreatedAt = now },
                    new ServiceCategory { Name = "Underbody Coating", BasePrice = 3000m, Description = "Anti-rust treatment for the chassis.", CreatedAt = now },
                    new ServiceCategory { Name = "Spark Plug Replacement", BasePrice = 900m, Description = "Installation of new plugs for better ignition.", CreatedAt = now },
                    new ServiceCategory { Name = "Power Steering Service", BasePrice = 1400m, Description = "Fluid replacement and pump check.", CreatedAt = now },
                    new ServiceCategory { Name = "Drive Belt Replacement", BasePrice = 1100m, Description = "Replacing worn serpentine belts.", CreatedAt = now },
                    new ServiceCategory { Name = "Sunroof Lubrication", BasePrice = 700m, Description = "Cleaning and greasing sunroof tracks.", CreatedAt = now },
                    new ServiceCategory { Name = "Wiper Motor Repair", BasePrice = 2200m, Description = "Fixing or replacing wiper motors.", CreatedAt = now },
                    new ServiceCategory { Name = "Exhaust System Repair", BasePrice = 3500m, Description = "Fixing leaks or replacing mufflers.", CreatedAt = now },
                    new ServiceCategory { Name = "Engine Carbon Cleaning", BasePrice = 4000m, Description = "Removing carbon deposits from intake.", CreatedAt = now },
                    new ServiceCategory { Name = "Door Lock Actuator Repair", BasePrice = 2800m, Description = "Fixing central locking issues.", CreatedAt = now },
                    new ServiceCategory { Name = "Alternator Replacement", BasePrice = 6500m, Description = "Installing a new charging unit.", CreatedAt = now },
                    new ServiceCategory { Name = "Starter Motor Service", BasePrice = 3200m, Description = "Overhauling the starter solenoid.", CreatedAt = now },
                    new ServiceCategory { Name = "General Full Inspection", BasePrice = 1500m, Description = "Pre-purchase or seasonal inspection.", CreatedAt = now }
                };
                context.ServiceCategories.AddRange(categoriesList);
                await context.SaveChangesAsync();
            }

            await Task.Delay(1000);
            var primaryCustomer = await userManager.FindByEmailAsync("customer@vehicle.com");
            var customers = new List<ApplicationUser?>
            {
                primaryCustomer, // Primary Customer must be first
                await userManager.FindByEmailAsync("sneha.customer@vehicle.com"),
                await userManager.FindByEmailAsync("mohit.customer@vehicle.com"),
                await userManager.FindByEmailAsync("pooja.customer@vehicle.com"),
                await userManager.FindByEmailAsync("rohit.customer@vehicle.com"),
                await userManager.FindByEmailAsync("neha.customer@vehicle.com"),
                await userManager.FindByEmailAsync("suresh.customer@vehicle.com"),
                await userManager.FindByEmailAsync("divya.customer@vehicle.com"),
                await userManager.FindByEmailAsync("aman.customer@vehicle.com"),
                await userManager.FindByEmailAsync("kiran.customer@vehicle.com")
            }.Where(u => u != null).ToList();

            if (primaryCustomer == null)
            {
                await Task.Delay(500);
                primaryCustomer = await userManager.FindByEmailAsync("customer@vehicle.com");
            }

            var technicians = new List<ApplicationUser?>
            {
                await userManager.FindByEmailAsync("tech@vehicle.com"),
                await userManager.FindByEmailAsync("priya.tech@vehicle.com"),
                await userManager.FindByEmailAsync("anita.tech@vehicle.com"),
                await userManager.FindByEmailAsync("vikram.tech@vehicle.com"),
                await userManager.FindByEmailAsync("kavita.tech@vehicle.com"),
                await userManager.FindByEmailAsync("arjun.tech@vehicle.com")
            }.Where(u => u != null).ToList();

            // ========== VEHICLES ==========
            if (primaryCustomer != null && !context.Vehicles.Any(v => v.UserId == primaryCustomer.Id))
            {
                var primaryCustomerVehicles = new List<Vehicle>
                {
                    new Vehicle { LicensePlate = "MH12AB3456", Make = "Maruti", Model = "Alto", Year = 2020, UserId = primaryCustomer.Id },
                    new Vehicle { LicensePlate = "DL01CD7890", Make = "Hyundai", Model = "i20", Year = 2021, UserId = primaryCustomer.Id },
                    new Vehicle { LicensePlate = "UP15XY1234", Make = "Toyota", Model = "Camry", Year = 2022, UserId = primaryCustomer.Id },
                };
                context.Vehicles.AddRange(primaryCustomerVehicles);
                await context.SaveChangesAsync();
            }

            if (!context.Vehicles.Any() && customers.Any() && customers.Count > 1)
            {
                var vehiclesList = new List<Vehicle>();
                
                if (primaryCustomer != null && !context.Vehicles.Any(v => v.UserId == primaryCustomer.Id))
                {
                    vehiclesList.AddRange(new List<Vehicle>
                    {
                        new Vehicle { LicensePlate = "MH12AB3456", Make = "Maruti", Model = "Alto", Year = 2020, UserId = primaryCustomer.Id },
                        new Vehicle { LicensePlate = "DL01CD7890", Make = "Hyundai", Model = "i20", Year = 2021, UserId = primaryCustomer.Id },
                        new Vehicle { LicensePlate = "UP15XY1234", Make = "Toyota", Model = "Camry", Year = 2022, UserId = primaryCustomer.Id },
                    });
                }
                
                if (customers.Count > 1)
                {
                    vehiclesList.AddRange(new List<Vehicle>
                    {
                        new Vehicle { LicensePlate = "KA03EF1234", Make = "Toyota", Model = "Camry", Year = 2019, UserId = customers[1]!.Id },
                        new Vehicle { LicensePlate = "TN09GH5678", Make = "Honda", Model = "City", Year = 2022, UserId = customers[1]!.Id },
                        
                        new Vehicle { LicensePlate = "GJ06IJ9012", Make = "Ford", Model = "EcoSport", Year = 2020, UserId = customers[2]!.Id },
                        
                        new Vehicle { LicensePlate = "UP14KL3456", Make = "Mahindra", Model = "XUV500", Year = 2021, UserId = customers[3]!.Id },
                        new Vehicle { LicensePlate = "RJ13MN7890", Make = "Tata", Model = "Nexon", Year = 2023, UserId = customers[3]!.Id },
                        
                        new Vehicle { LicensePlate = "WB15OP1234", Make = "Volkswagen", Model = "Polo", Year = 2019, UserId = customers[4]!.Id },
                        
                        new Vehicle { LicensePlate = "MP16QR5678", Make = "Nissan", Model = "Micra", Year = 2020, UserId = customers[5]!.Id },
                        new Vehicle { LicensePlate = "BR17ST9012", Make = "Skoda", Model = "Rapid", Year = 2021, UserId = customers[5]!.Id },
                        
                        new Vehicle { LicensePlate = "AP18UV3456", Make = "Maruti", Model = "Swift", Year = 2022, UserId = customers[6]!.Id },
                        
                        new Vehicle { LicensePlate = "HR19WX7890", Make = "Hyundai", Model = "Creta", Year = 2023, UserId = customers[7]!.Id },
                        
                        new Vehicle { LicensePlate = "PB20YZ1234", Make = "Toyota", Model = "Innova", Year = 2020, UserId = customers[8]!.Id },
                        
                        new Vehicle { LicensePlate = "KL21AA5678", Make = "Honda", Model = "Amaze", Year = 2021, UserId = customers[9]!.Id },
                        new Vehicle { LicensePlate = "OR22BB9012", Make = "Mahindra", Model = "Scorpio", Year = 2022, UserId = customers[9]!.Id }
                    });
                }
                
                if (vehiclesList.Any())
                {
                    context.Vehicles.AddRange(vehiclesList);
                    await context.SaveChangesAsync();
                }
            }

            var categories = context.ServiceCategories.ToList();
            var parts = context.Parts.ToList();
            var vehicles = context.Vehicles.ToList();

            if (!categories.Any() || !vehicles.Any() || !technicians.Any() || !customers.Any())
            {
                return;
            }

            if (!context.ServiceRequests.Any())
            {
                var random = new Random(42);
                var serviceRequests = new List<ServiceRequest>();
                var now = DateTime.UtcNow;

                var techWeights = new Dictionary<int, int>
                {
                    { 0, 8 },
                    { 1, 6 },
                    { 2, 4 },
                    { 3, 5 },
                    { 4, 3 },
                    { 5, 2 }
                };

                var highValueCategories = categories
                    .Where(c => c.BasePrice >= 3000m)
                    .OrderByDescending(c => c.BasePrice)
                    .ToList();
                var mediumValueCategories = categories
                    .Where(c => c.BasePrice >= 1000m && c.BasePrice < 3000m)
                    .ToList();
                var lowValueCategories = categories
                    .Where(c => c.BasePrice < 1000m)
                    .ToList();

                ApplicationUser? GetWeightedTechnician()
                {
                    var totalWeight = techWeights.Values.Sum();
                    var roll = random.Next(totalWeight);
                    var current = 0;
                    foreach (var kvp in techWeights)
                    {
                        current += kvp.Value;
                        if (roll < current && kvp.Key < technicians.Count)
                        {
                            return technicians[kvp.Key];
                        }
                    }
                    return technicians[0];
                }

                ServiceCategory GetWeightedCategory()
                {
                    var roll = random.Next(100);
                    if (roll < 40 && highValueCategories.Any())
                        return highValueCategories[random.Next(highValueCategories.Count)];
                    else if (roll < 70 && mediumValueCategories.Any())
                        return mediumValueCategories[random.Next(mediumValueCategories.Count)];
                    else if (lowValueCategories.Any())
                        return lowValueCategories[random.Next(lowValueCategories.Count)];
                    return categories[random.Next(categories.Count)];
                }

                var monthlyDistribution = new List<(int monthOffset, int serviceCount)>
                {
                    (0, 8),   // Current month - most services
                    (1, 12),  // Last month - peak
                    (2, 10),  // 2 months ago
                    (3, 8),   // 3 months ago
                    (4, 6),   // 4 months ago
                    (5, 4),   // 5 months ago
                    (6, 2)    // 6 months ago - least
                };

                int serviceIndex = 0;
                foreach (var (monthOffset, count) in monthlyDistribution)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var vehicle = vehicles[random.Next(vehicles.Count)];
                        var customer = customers.FirstOrDefault(c => c!.Id == vehicle.UserId);
                        if (customer == null) continue;
                        
                        var tech = GetWeightedTechnician();
                        if (tech == null) continue;
                        
                        var category = GetWeightedCategory();
                        var baseDate = now.AddMonths(-monthOffset);
                        var requestedDate = baseDate.AddDays(-random.Next(0, 15));
                        var completionDate = requestedDate.AddDays(random.Next(1, 5));

                        var service = new ServiceRequest
                        {
                            Description = GetRandomServiceDescription(category.Name),
                            Status = "Closed",
                            Priority = random.Next(10) < 2 ? "Urgent" : "Normal",
                            RequestedDate = requestedDate,
                            CompletionDate = completionDate,
                            ServiceCategoryId = category.Id,
                            CustomerId = customer.Id,
                            VehicleId = vehicle.Id,
                            TechnicianId = tech.Id
                        };
                        serviceRequests.Add(service);
                        serviceIndex++;
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    var vehicle = vehicles[random.Next(vehicles.Count)];
                    var customer = customers.FirstOrDefault(c => c!.Id == vehicle.UserId);
                    if (customer == null) continue;
                    
                    var tech = GetWeightedTechnician();
                    if (tech == null) continue;
                    
                    var category = GetWeightedCategory();
                    var requestedDate = now.AddDays(-random.Next(30, 60));
                    var completionDate = requestedDate.AddDays(random.Next(1, 4));

                    var service = new ServiceRequest
                    {
                        Description = GetRandomServiceDescription(category.Name),
                        Status = "Completed",
                        Priority = random.Next(10) < 3 ? "Urgent" : "Normal",
                        RequestedDate = requestedDate,
                        CompletionDate = completionDate,
                        ServiceCategoryId = category.Id,
                        CustomerId = customer.Id,
                        VehicleId = vehicle.Id,
                        TechnicianId = tech.Id
                    };
                    serviceRequests.Add(service);
                }

                for (int i = 0; i < 8; i++)
                {
                    var vehicle = vehicles[random.Next(vehicles.Count)];
                    var customer = customers.FirstOrDefault(c => c!.Id == vehicle.UserId);
                    if (customer == null) continue;
                    
                    var tech = GetWeightedTechnician();
                    if (tech == null) continue;
                    
                    var category = GetWeightedCategory();
                    var requestedDate = now.AddDays(-random.Next(5, 15));

                    var service = new ServiceRequest
                    {
                        Description = GetRandomServiceDescription(category.Name),
                        Status = "In Progress",
                        Priority = random.Next(10) < 3 ? "Urgent" : "Normal",
                        RequestedDate = requestedDate,
                        ServiceCategoryId = category.Id,
                        CustomerId = customer.Id,
                        VehicleId = vehicle.Id,
                        TechnicianId = tech.Id
                    };
                    serviceRequests.Add(service);
                }

                for (int i = 0; i < 7; i++)
                {
                    var vehicle = vehicles[random.Next(vehicles.Count)];
                    var customer = customers.FirstOrDefault(c => c!.Id == vehicle.UserId);
                    if (customer == null) continue;
                    
                    var tech = GetWeightedTechnician();
                    if (tech == null) continue;
                    
                    var category = GetWeightedCategory();
                    var requestedDate = now.AddDays(-random.Next(1, 7));

                    var service = new ServiceRequest
                    {
                        Description = GetRandomServiceDescription(category.Name),
                        Status = "Assigned",
                        Priority = random.Next(10) < 2 ? "Urgent" : "Normal",
                        RequestedDate = requestedDate,
                        ServiceCategoryId = category.Id,
                        CustomerId = customer.Id,
                        VehicleId = vehicle.Id,
                        TechnicianId = tech.Id
                    };
                    serviceRequests.Add(service);
                }

                for (int i = 0; i < 10; i++)
                {
                    var vehicle = vehicles[random.Next(vehicles.Count)];
                    var customer = customers.FirstOrDefault(c => c!.Id == vehicle.UserId);
                    if (customer == null) continue;
                    var category = GetWeightedCategory();
                    var requestedDate = now.AddDays(-random.Next(0, 3));

                    var service = new ServiceRequest
                    {
                        Description = GetRandomServiceDescription(category.Name),
                        Status = "Requested",
                        Priority = random.Next(10) < 2 ? "Urgent" : "Normal",
                        RequestedDate = requestedDate,
                        ServiceCategoryId = category.Id,
                        CustomerId = customer.Id,
                        VehicleId = vehicle.Id
                    };
                    serviceRequests.Add(service);
                }

                context.ServiceRequests.AddRange(serviceRequests);
                await context.SaveChangesAsync();

                // ========== SERVICE REQUEST PARTS (Parts Used) ==========
                var savedServices = context.ServiceRequests
                    .Where(s => s.Status == "Completed" || s.Status == "Closed")
                    .ToList();

                // Sort parts by price (high to low) for varied revenue
                var expensiveParts = parts.OrderByDescending(p => p.UnitPrice).Take(parts.Count / 2).ToList();
                var affordableParts = parts.OrderBy(p => p.UnitPrice).Take(parts.Count / 2).ToList();

                var serviceRequestParts = new List<ServiceRequestPart>();
                foreach (var service in savedServices)
                {
                    var category = categories.First(c => c.Id == service.ServiceCategoryId);
                    var isHighValueService = category.BasePrice >= 3000m;
                    
                    var partsToUse = new List<Part>();
                    var numParts = random.Next(1, 5);
                    
                    if (isHighValueService)
                    {
                        // 70% chance of expensive parts for high-value services
                        for (int i = 0; i < numParts; i++)
                        {
                            if (random.Next(100) < 70 && expensiveParts.Any())
                                partsToUse.Add(expensiveParts[random.Next(expensiveParts.Count)]);
                            else if (affordableParts.Any())
                                partsToUse.Add(affordableParts[random.Next(affordableParts.Count)]);
                        }
                    }
                    else
                    {
                        // Regular services get mix of parts
                        var allParts = parts.OrderBy(x => random.Next()).Take(numParts).ToList();
                        partsToUse.AddRange(allParts);
                    }
                    
                    foreach (var part in partsToUse)
                    {
                        var quantity = isHighValueService ? random.Next(2, 5) : random.Next(1, 3);
                        serviceRequestParts.Add(new ServiceRequestPart
                        {
                            ServiceRequestId = service.Id,
                            PartId = part.Id,
                            QuantityUsed = quantity
                        });
                    }
                }
                context.ServiceRequestParts.AddRange(serviceRequestParts);
                await context.SaveChangesAsync();

                var servicesWithParts = context.ServiceRequests
                    .Where(s => s.Status == "Completed" || s.Status == "Closed")
                    .ToList();

                var invoices = new List<Invoice>();
                foreach (var service in servicesWithParts)
                {
                    var category = categories.First(c => c.Id == service.ServiceCategoryId);
                    var usedPartsForService = serviceRequestParts.Where(sp => sp.ServiceRequestId == service.Id).ToList();
                    var partsCost = usedPartsForService.Sum(sp =>
                    {
                        var part = parts.First(p => p.Id == sp.PartId);
                        return part.UnitPrice * sp.QuantityUsed;
                    });
                    var totalAmount = category.BasePrice + partsCost;

                    invoices.Add(new Invoice
                    {
                        ServiceRequestId = service.Id,
                        TotalAmount = totalAmount,
                        PaymentStatus = service.Status == "Closed" ? "Paid" : "Pending",
                        IssuedDate = service.CompletionDate ?? service.RequestedDate
                    });
                }
                context.Invoices.AddRange(invoices);
                await context.SaveChangesAsync();
            }
        }

        private static async Task<ApplicationUser?> CreateUserWithRole(UserManager<ApplicationUser> userManager, string fullName, string email, string password, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    return user;
                }
            }
            return await userManager.FindByEmailAsync(email);
        }

        private static string GetRandomServiceDescription(string serviceCategory)
        {
            var descriptions = new Dictionary<string, List<string>>
            {
                ["Express Wash"] = new List<string> { "Quick exterior wash needed", "Car needs immediate cleaning", "Regular maintenance wash" },
                ["Standard Oil Change"] = new List<string> { "Regular oil change service", "Engine oil replacement needed", "Scheduled maintenance oil change" },
                ["Brake System Overhaul"] = new List<string> { "Brake pads making noise", "Brake pedal feels soft", "Complete brake inspection required", "Brake system not responding properly" },
                ["AC Gas Refill"] = new List<string> { "AC not cooling properly", "Air conditioning needs refill", "AC gas leak detected", "AC compressor not working" },
                ["Engine Diagnostic Scan"] = new List<string> { "Check engine light is on", "Engine making strange noises", "Performance issues detected", "Need diagnostic check" },
                ["Wheel Alignment"] = new List<string> { "Vehicle pulling to one side", "Uneven tire wear noticed", "Steering wheel vibration", "Alignment check needed" },
                ["Battery Health Check"] = new List<string> { "Battery not holding charge", "Car not starting properly", "Battery warning light on", "Need battery replacement check" },
                ["Full Preventive Maintenance"] = new List<string> { "Scheduled maintenance service", "60-point inspection needed", "Regular service checkup", "Comprehensive vehicle check" },
                ["Transmission Flush"] = new List<string> { "Transmission fluid change needed", "Gear shifting issues", "Transmission service required", "Rough gear changes" },
                ["Suspension Tuning"] = new List<string> { "Car bouncing excessively", "Suspension making noise", "Ride quality issues", "Suspension inspection needed" }
            };

            if (descriptions.ContainsKey(serviceCategory))
            {
                var categoryDescriptions = descriptions[serviceCategory];
                return categoryDescriptions[new Random().Next(categoryDescriptions.Count)];
            }

            // Default descriptions
            var defaultDescriptions = new List<string>
            {
                "Regular maintenance service required",
                "Service requested for vehicle checkup",
                "Need professional inspection",
                "Scheduled service appointment",
                "Vehicle service needed",
                "Maintenance check required",
                "Service request for vehicle"
            };
            return defaultDescriptions[new Random().Next(defaultDescriptions.Count)];
        }
    }
}
