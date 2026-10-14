namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback rate cycle is in its life cycle.</summary>
public enum FallbackRateCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A fallback rate cycle as the rate team keeps it.</summary>
public sealed class FallbackRateCycle(string code, string name, double amount, string country, string category, List<string> tags, Dictionary<string, string> attributes, string source, int limit, FallbackRateCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Country { get; set; } = country;
    public string Category { get; set; } = category;
    public List<string> Tags { get; set; } = tags;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Source { get; set; } = source;
    public int Limit { get; set; } = limit;
    public FallbackRateCycleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback rate cycle in status created.</summary>
    public static FallbackRateCycle NewFallbackRateCycle(string code, string name)
    {
        return new FallbackRateCycle(
            code,
            name,
            0.0,
            "",
            "",
            new List<string>(),
            new Dictionary<string, string>(),
            "",
            0,
            FallbackRateCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRateCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRateCycleStatus next)
    {
        switch (Status)
        {
            case FallbackRateCycleStatus.Created:
                switch (next)
                {
                    case FallbackRateCycleStatus.Published:
                    case FallbackRateCycleStatus.Retired:
                        return true;
                }
                return false;
            case FallbackRateCycleStatus.Validated:
                switch (next)
                {
                    case FallbackRateCycleStatus.Retired:
                        return true;
                }
                return false;
            case FallbackRateCycleStatus.Published:
                return next == FallbackRateCycleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FallbackRateCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback rate cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Limit * Amount, 2);
    }

    public FallbackRateCycle Copy()
    {
        return new FallbackRateCycle(Code, Name, Amount, Country, Category, Tags, Attributes, Source, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
