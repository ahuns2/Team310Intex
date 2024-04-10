using System;
using System.Collections.Generic;

namespace testingINTEX.Models;

public partial class Transaction
{
    public int? TransactionId { get; set; }

    public int? ProductId { get; set; }

    public int? Qty { get; set; }

    public int? Rating { get; set; }

    public virtual Product? Product { get; set; }

    public virtual Order? TransactionNavigation { get; set; }
}
