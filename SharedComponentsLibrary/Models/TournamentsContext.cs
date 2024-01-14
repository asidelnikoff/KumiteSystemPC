using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SharedComponentsLibrary.Models;

public partial class TournamentsContext : DbContext
{
    public TournamentsContext(string databaseName)
    {
        connectionString = $"Data Source={databaseName}";
    }

    string connectionString;

    public TournamentsContext(DbContextOptions<TournamentsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Competitor> Competitors { get; set; }

    public virtual DbSet<CompetitorCategory> CompetitorCategories { get; set; }

    public virtual DbSet<Match> Matches { get; set; }

    public virtual DbSet<Round> Rounds { get; set; }

    public virtual DbSet<Tournament> Tournaments { get; set; }

    public virtual DbSet<Winner> Winners { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite(connectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.HasOne(d => d.TournamentNavigation).WithMany(p => p.Categories).HasForeignKey(d => d.Tournament);
        });

        modelBuilder.Entity<Competitor>(entity =>
        {
            entity.ToTable("Competitor");

            entity.Property(e => e.Id)
                .HasColumnName("ID");
            entity.Property(e => e.IsBye)
                .HasDefaultValueSql("0")
                .HasColumnName("isBye");
            entity.Property(e => e.Status).HasDefaultValueSql("0");
        });

        modelBuilder.Entity<CompetitorCategory>(entity =>
        {
            entity.ToTable("CompetitorCategory");

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.Property(e => e.CompetitorStatus)
               .HasDefaultValueSql("0")
               .HasColumnName("CompetitorStatus");

            entity.HasOne(d => d.CategoryNavigation).WithMany(p => p.CompetitorCategories)
                .HasForeignKey(d => d.Category)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.CompetitorNavigation).WithMany(p => p.CompetitorCategories)
                .HasForeignKey(d => d.Competitor)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Round, e.Category });

            entity.ToTable("Match");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Aka).HasColumnName("AKA");
            entity.Property(e => e.AkaC1)
                .HasDefaultValueSql("0")
                .HasColumnName("AKA_C1");
            entity.Property(e => e.AkaC2)
                .HasDefaultValueSql("0")
                .HasColumnName("AKA_C2");
            entity.Property(e => e.AkaScore)
                .HasDefaultValueSql("0")
                .HasColumnName("AKA_score");
            entity.Property(e => e.Ao).HasColumnName("AO");
            entity.Property(e => e.AoC1)
                .HasDefaultValueSql("0")
                .HasColumnName("AO_C1");
            entity.Property(e => e.AoC2)
                .HasDefaultValueSql("0")
                .HasColumnName("AO_C2");
            entity.Property(e => e.AoScore)
                .HasDefaultValueSql("0")
                .HasColumnName("AO_score");
            entity.Property(e => e.IsFinished)
                .HasDefaultValueSql("0")
                .HasColumnName("isFinished");
            entity.Property(e => e.Senshu).HasDefaultValueSql("0");

            entity.HasOne(d => d.AkaNavigation).WithMany(p => p.MatchAkaNavigations).HasForeignKey(d => d.Aka);

            entity.HasOne(d => d.AoNavigation).WithMany(p => p.MatchAoNavigations).HasForeignKey(d => d.Ao);

            entity.HasOne(d => d.LooserNavigation).WithMany(p => p.MatchLooserNavigations).HasForeignKey(d => d.Looser);

            entity.HasOne(d => d.WinnerNavigation).WithMany(p => p.MatchWinnerNavigations).HasForeignKey(d => d.Winner);

            entity.HasOne(d => d.RoundNavigation).WithMany(p => p.Matches)
                .HasForeignKey(d => new { d.Round, d.Category })
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Round>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Category });

            entity.ToTable("Round");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Repechage).HasDefaultValueSql("-1");

            entity.HasOne(d => d.CategoryNavigation).WithMany(p => p.Rounds)
                .HasForeignKey(d => d.Category)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.ToTable("Tournament");

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<Winner>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");

            entity.HasOne(d => d.CategoryNavigation).WithMany(p => p.Winners)
                .HasForeignKey(d => d.Category)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.CompetitorNavigation).WithMany(p => p.Winners)
                .HasForeignKey(d => d.Competitor)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
