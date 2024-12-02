namespace TaskManagerAPI.Database
{
    using Microsoft.EntityFrameworkCore;
    using TaskManagerAPI.Model;

    public class TaskContext : DbContext
    {
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }


        public TaskContext(DbContextOptions<TaskContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tasks>().HasKey(t => t.Id);
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Tasks>()
            .HasOne(t => t.AssignedUser)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Tasks && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var task = (Tasks)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    task.CreatedAt = DateTime.UtcNow;
                }


            }

            return await base.SaveChangesAsync(cancellationToken);
        }

    }
}
