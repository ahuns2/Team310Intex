using System;
using System.Collections.Generic;

namespace testingINTEX.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? CountryOfResidence { get; set; }

    public char? Gender { get; set; }

    public decimal? Age { get; set; }

    public Guid? AspUserId { get; set; }

    public int? Recommendation1 { get; set; }

    public int? Recommendation2 { get; set; }

    public int? Recommendation3 { get; set; }

    public int? Recommendation4 { get; set; }

    public int? Recommendation5 { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
