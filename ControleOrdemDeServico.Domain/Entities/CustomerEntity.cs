using System;
using System.Collections.Generic;
using System.Text;

namespace OsService.Domain.Entities
{
    public class CustomerEntity
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string? Phone { get; init; }
        public string? Email { get; init; }
        public string? Document { get; init; }
        public DateTime CreatedAt { get; init; }

    }
}
