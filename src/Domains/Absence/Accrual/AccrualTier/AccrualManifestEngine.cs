namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual manifest calculation.</summary>
public interface IAccrualManifestAdjuster
{
    string Describe();
    double Adjust(AccrualManifest item, double valueValue);
}

public sealed record PercentAccrualManifestAdjuster(double Percent) : IAccrualManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAccrualManifestAdjuster(double Minimum) : IAccrualManifestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAccrualManifestAdjuster(double Cap) : IAccrualManifestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAccrualManifestAdjuster(Registry Flags, string Flag, double Percent) : IAccrualManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AccrualManifestLimitOffsetAdjuster(double Weight) : IAccrualManifestAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(AccrualManifest item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record TieredAccrualManifestAdjuster(List<double> Bounds, List<double> Rates) : IAccrualManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualManifest item, double valueValue)
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

/// <summary>Runs the accrual manifest steps in order and keeps a trace of each.</summary>
public sealed class AccrualManifestEngine(List<IAccrualManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualManifestEngine DefaultAccrualManifestEngine(Registry flags)
    {
        return new AccrualManifestEngine(
            new List<IAccrualManifestAdjuster>
            {
                new PercentAccrualManifestAdjuster(-1.2),
                new MinimumAccrualManifestAdjuster(36.5),
                new CapAccrualManifestAdjuster(35007.0),
                new FlaggedAccrualManifestAdjuster(flags, "accrual-manifest-scheduled", -9.7),
                new AccrualManifestLimitOffsetAdjuster(1.6),
                new TieredAccrualManifestAdjuster(
                    new List<double> { 4851.0, 6363.0, 7746.0, 8752.0 },
                    new List<double> { 0.87, 0.99, 1.1, 0.92 }),
            },
            new List<string>(),
            3.4,
            15528.6);
    }

    public double Price(AccrualManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<AccrualManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
