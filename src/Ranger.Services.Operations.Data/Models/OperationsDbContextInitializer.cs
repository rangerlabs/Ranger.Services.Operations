using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ranger.Common;

namespace Ranger.Services.Operations.Data {
    public class OperationsDbContextInitializer : IOperationsDbContextInitializer {
        private readonly OperationsDbContext context;

        public OperationsDbContextInitializer (OperationsDbContext context) {
            this.context = context;
        }

        public bool EnsureCreated () {
            return context.Database.EnsureCreated ();
        }

        public void Migrate () {
            context.Database.Migrate ();
        }
    }

    public interface IOperationsDbContextInitializer {
        bool EnsureCreated ();
        void Migrate ();
    }
}