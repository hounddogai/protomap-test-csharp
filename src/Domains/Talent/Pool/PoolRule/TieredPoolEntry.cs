namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pool entry is in its life cycle.</summary>
public enum TieredPoolEntryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One tiered pool entry, with what the planners need to act on it.</summary>
public sealed class TieredPoolEntry(string code, string name, string category, double maximum, string notes, double budget, string location, string currency, string country, TieredPoolEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public double Maximum { get; set; } = maximum;
    public string Notes { get; set; } = notes;
    public double Budget { get; set; } = budget;
    public string Location { get; set; } = location;
    public string Currency { get; set; } = currency;
    public string Country { get; set; } = country;
    public TieredPoolEntryStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pool entry in status created.</summary>
    public static TieredPoolEntry NewTieredPoolEntry(string code, string name)
    {
        return new TieredPoolEntry(code, name, "", 0.0, "", 0.0, "", "", "", TieredPoolEntryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPoolEntryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPoolEntryStatus next)
    {
        switch (Status)
        {
            case TieredPoolEntryStatus.Created:
                return next == TieredPoolEntryStatus.Retired;
            case TieredPoolEntryStatus.Validated:
                switch (next)
                {
                    case TieredPoolEntryStatus.Retired:
                        return true;
                }
                return false;
            case TieredPoolEntryStatus.Published:
                switch (next)
                {
                    case TieredPoolEntryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredPoolEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pool entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedValue()
    {
        return Math.Round(Budget - Maximum, 2);
    }

    public TieredPoolEntry Copy()
    {
        return new TieredPoolEntry(
            Code,
            Name,
            Category,
            Maximum,
            Notes,
            Budget,
            Location,
            Currency,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
