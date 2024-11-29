using FFMpegCore.WebApi.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public required DbSet<JobLog> JobLogs { get; set; }
    public required DbSet<RenderLog> RenderLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RenderLog>(entity =>
        {
            entity.HasKey(e => e.VideoId); // Define VideoId as the primary key
        });

        base.OnModelCreating(modelBuilder);
    }
}
