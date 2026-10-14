namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional balance digest calculation.</summary>
public interface IRegionalBalanceDigestModifier
{
    string Describe();
    double Adjust(RegionalBalanceDigest item, double valueValue);
}

public sealed record TieredRegionalBalanceDigestAdjuster(List<double> Bounds, List<double> Rates) : IRegionalBalanceDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalBalanceDigest item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record CapRegionalBalanceDigestAdjuster(double Cap) : IRegionalBalanceDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalBalanceDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRegionalBalanceDigestAdjuster(double Percent) : IRegionalBalanceDigestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalBalanceDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalBalanceDigestAdjuster(double Minimum) : IRegionalBalanceDigestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalBalanceDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RegionalBalanceDigestCostOffsetAdjuster(double Weight) : IRegionalBalanceDigestModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(RegionalBalanceDigest item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record FlaggedRegionalBalanceDigestAdjuster(Registry Flags, string Flag, double Percent) : IRegionalBalanceDigestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalBalanceDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional balance digest steps in order and keeps a trace of each.</summary>
public sealed class RegionalBalanceDigestEngine(List<IRegionalBalanceDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalBalanceDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalBalanceDigestEngine DefaultRegionalBalanceDigestEngine(Registry flags)
    {
        return new RegionalBalanceDigestEngine(
            new List<IRegionalBalanceDigestModifier>
            {
                new TieredRegionalBalanceDigestAdjuster(
                    new List<double> { 4017.0, 6330.0, 6676.0, 7759.0 },
                    new List<double> { 0.99, 0.96, 1.16, 0.83 }),
                new CapRegionalBalanceDigestAdjuster(40315.0),
                new PercentRegionalBalanceDigestAdjuster(21.4),
                new MinimumRegionalBalanceDigestAdjuster(10.5),
                new RegionalBalanceDigestCostOffsetAdjuster(1.19),
                new FlaggedRegionalBalanceDigestAdjuster(flags, "regional-balance-digest-seasonal", 2.5),
            },
            new List<string>(),
            0.3,
            34334.8);
    }

    public double Price(RegionalBalanceDigest item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalBalanceDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
