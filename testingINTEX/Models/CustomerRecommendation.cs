using System;
using System.Collections.Generic;

namespace testingINTEX.Models;

public partial class CustomerRecommendation
{
    public int CustomerId { get; set; }

    public int? Recommendation1 { get; set; }

    public int? Recommendation2 { get; set; }

    public int? Recommendation3 { get; set; }

    public int? Recommendation4 { get; set; }

    public int? Recommendation5 { get; set; }
}
