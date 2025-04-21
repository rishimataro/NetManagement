using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetManagement.Models;

namespace NetManagement.Data
{
    public class NetManagementContext : DbContext
    {
        public NetManagementContext()
        {
        }

        public NetManagementContext (DbContextOptions<NetManagementContext> options)
            : base(options)
        {
        }

        public DbSet<NetManagement.Models.Computer> Computer { get; set; } = default!;
        public DbSet<NetManagement.Models.User> User { get; set; } = default!;
        public DbSet<NetManagement.Models.Order> Order { get; set; } = default!;
    }
}
