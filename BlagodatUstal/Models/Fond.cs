using System;
using System.Collections.Generic;

namespace BlagodatUstal.Models;

public partial class Fond
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? Balance { get; set; }
}
