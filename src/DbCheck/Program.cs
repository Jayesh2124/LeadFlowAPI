using System;
using Microsoft.EntityFrameworkCore;
using LeadFlow.Infrastructure.Persistence;
using LeadFlow.Domain.Entities;
using System.Linq;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var connectionString = "Host=localhost;Port=5432;Database=leadflow;Username=postgres;Password=Password@123";
var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseNpgsql(connectionString);
using var db = new AppDbContext(optionsBuilder.Options);

var templates = db.EmailTemplates.ToList();
Console.WriteLine($"Total templates: {templates.Count}");
foreach(var t in templates) Console.WriteLine($"- {t.Id} | {t.Name} | UserId: {t.UserId}");

var smtps = db.UserSmtpSettings.ToList();
Console.WriteLine($"\nTotal SMTPs: {smtps.Count}");
foreach(var s in smtps) Console.WriteLine($"- {s.UserId} | {s.Host} | {s.FromEmail}");
