using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace sistemasconsultas.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Consulta> Consultas => Set<Consulta>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Medico>(e =>
        {
            // Um mesmo usuário não pode cadastrar dois médicos com o mesmo CRM
            e.HasIndex(m => new { m.UserId, m.Crm }).IsUnique();

            e.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Paciente>(e =>
        {
            // Um mesmo usuário não pode cadastrar dois pacientes com o mesmo CPF
            e.HasIndex(p => new { p.UserId, p.Cpf }).IsUnique();

            e.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Consulta>(e =>
        {
            // Horário da clínica guardado como está, sem conversão de fuso
            e.Property(c => c.Inicio).HasColumnType("timestamp without time zone");
            e.Property(c => c.Fim).HasColumnType("timestamp without time zone");

            // Status guardado como texto legível ("Agendada", "Cancelada"...)
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

            e.HasIndex(c => new { c.UserId, c.Inicio });
            e.HasIndex(c => new { c.MedicoId, c.Inicio });

            e.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // O banco impede apagar médico ou paciente que tenham consultas
            e.HasOne(c => c.Medico)
                .WithMany()
                .HasForeignKey(c => c.MedicoId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(c => c.Paciente)
                .WithMany()
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}