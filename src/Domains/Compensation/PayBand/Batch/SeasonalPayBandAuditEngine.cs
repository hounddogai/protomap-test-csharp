namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal pay band audit calculation.</summary>
public interface ISeasonalPayBandAuditFactor
{
    string Describe();
    double Adjust(SeasonalPayBandAudit item, double valueValue);
}

public sealed record TieredSeasonalPayBandAuditAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalPayBandAuditFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalPayBandAudit item, double valueValue)
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

public sealed record PercentSeasonalPayBandAuditAdjuster(double Percent) : ISeasonalPayBandAuditFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalPayBandAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalPayBandAuditThresholdOffsetAdjuster(double Weight) : ISeasonalPayBandAuditFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(SeasonalPayBandAudit item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record CapSeasonalPayBandAuditAdjuster(double Cap) : ISeasonalPayBandAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalPayBandAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSeasonalPayBandAuditAdjuster(double Minimum) : ISeasonalPayBandAuditFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalPayBandAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSeasonalPayBandAuditAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalPayBandAuditFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalPayBandAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal pay band audit steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPayBandAuditEngine(List<ISeasonalPayBandAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPayBandAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPayBandAuditEngine DefaultSeasonalPayBandAuditEngine(Registry flags)
    {
        return new SeasonalPayBandAuditEngine(
            new List<ISeasonalPayBandAuditFactor>
            {
                new TieredSeasonalPayBandAuditAdjuster(
                    new List<double> { 975.0, 2392.0, 3024.0, 9236.0 },
                    new List<double> { 1.19, 1.2, 0.82, 1.01 }),
                new PercentSeasonalPayBandAuditAdjuster(-16.2),
                new SeasonalPayBandAuditThresholdOffsetAdjuster(1.31),
                new CapSeasonalPayBandAuditAdjuster(16969.0),
                new MinimumSeasonalPayBandAuditAdjuster(36.8),
                new FlaggedSeasonalPayBandAuditAdjuster(flags, "seasonal-pay-band-audit-regional", 6.3),
            },
            new List<string>(),
            3.1,
            1916.2);
    }

    public double Compute(SeasonalPayBandAudit item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPayBandAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
