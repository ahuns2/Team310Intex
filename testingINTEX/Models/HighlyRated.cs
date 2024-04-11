using System;
using System.Collections.Generic;

namespace testingINTEX.Models;

public partial class HighlyRated
{
    public int HighlyRatedId { get; set; }

    public int? ProductId { get; set; }

    public decimal? AverageRatings { get; set; }
}
