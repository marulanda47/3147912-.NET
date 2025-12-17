using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace techNova.Data
{
    // DbContext SOLO para Identity
    public class AuthDbContext : IdentityDbContext<IdentityUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 1) Identity configura sus tablas
            base.OnModelCreating(builder);

            // 2) .NET 10: sacamos la entidad de passkeys para evitar el error de PK
            builder.Ignore<IdentityPasskeyData>();
        }
    }
}
