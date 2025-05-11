using System;
using System.Collections.Generic;

namespace BlagodatUstal.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string Photopath { get; set; } = null!;

    public DateOnly Birthday { get; set; }

    public string Country { get; set; } = null!;

    public bool? Active { get; set; }
}
