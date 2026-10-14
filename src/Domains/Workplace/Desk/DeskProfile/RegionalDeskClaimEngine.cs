namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional desk claim calculation.</summary>
public interface IRegionalDeskClaimFactor
{
    string Describe();
    double Adjust(RegionalDeskClaim item, double valueValue);
}

public sealed record RegionalDeskClaimExpiresAtOffsetAdjuster(double Weight) : IRegionalDeskClaimFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(RegionalDeskClaim item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record PercentRegionalDeskClaimAdjuster(double Percent) : IRegionalDeskClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalDeskClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamRegionalDeskClaimAdjuster(Dictionary<string, double> Factors) : IRegionalDeskClaimFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(RegionalDeskClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRegionalDeskClaimAdjuster(double Minimum) : IRegionalDeskClaimFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalDeskClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRegionalDeskClaimAdjuster(Registry Flags, string Flag, double Percent) : IRegionalDeskClaimFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalDeskClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional desk claim steps in order and keeps a trace of each.</summary>
public sealed class RegionalDeskClaimEngine(List<IRegionalDeskClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalDeskClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalDeskClaimEngine DefaultRegionalDeskClaimEngine(Registry flags)
    {
        return new RegionalDeskClaimEngine(
            new List<IRegionalDeskClaimFactor>
            {
                new RegionalDeskClaimExpiresAtOffsetAdjuster(1.67),
                new PercentRegionalDeskClaimAdjuster(-1.3),
                new TeamRegionalDeskClaimAdjuster(new Dictionary<string, double> { ["south"] = 0.78 }),
                new MinimumRegionalDeskClaimAdjuster(31.5),
                new FlaggedRegionalDeskClaimAdjuster(flags, "regional-desk-claim-legacy", 6.1),
            },
            new List<string>(),
            0.5,
            70592.1);
    }

    public double Compute(RegionalDeskClaim item)
    {
        var current = item.AdjustedTotal();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RegionalDeskClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
