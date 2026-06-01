using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PhoneBookDI.Models;
namespace PhoneBookDI.Models;

public partial class PhoneBookDbEvseev2307aContext : DbContext
{
    public PhoneBookDbEvseev2307aContext()
    {
    }

    public PhoneBookDbEvseev2307aContext(DbContextOptions<PhoneBookDbEvseev2307aContext> options)
        : base(options)
    {
    }

    public DbSet<Contact> Contacts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#warning To protect potentially sensitive information...
        optionsBuilder.UseSqlite("Data Source=PhoneBookDB_Evseev_2307a.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}