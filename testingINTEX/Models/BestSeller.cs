using System;
using System.Collections.Generic;

namespace testingINTEX.Models;

public partial class BestSeller
{
    public int BestSellersId { get; set; }

    public int? ProductId { get; set; }

    public int? TotalQtySold { get; set; }
}
