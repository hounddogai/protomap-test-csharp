namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh quota calculation.</summary>
public interface IRefreshQuotaStep
{
    string Describe();
    double Adjust(RefreshQuota item, double valueValue);
}

public sealed record FlaggedRefreshQuotaAdjuster(Registry Flags, string Flag, double Percent) : IRefreshQuotaStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRefreshQuotaAdjuster(double Cap) : IRefreshQuotaStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRefreshQuotaAdjuster(double Percent) : IRefreshQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RefreshQuotaHoursOffsetAdjuster(double Weight) : IRefreshQuotaStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(RefreshQuota item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record CountryRefreshQuotaAdjuster(Dictionary<string, double> Factors) : IRefreshQuotaStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RefreshQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRefreshQuotaAdjuster(List<double> Bounds, List<double> Rates) : IRefreshQuotaStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshQuota item, double valueValue)
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

/// <summary>Runs the refresh quota steps in order and keeps a trace of each.</summary>
public sealed class RefreshQuotaEngine(List<IRefreshQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshQuotaEngine DefaultRefreshQuotaEngine(Registry flags)
    {
        return new RefreshQuotaEngine(
            new List<IRefreshQuotaStep>
            {
                new FlaggedRefreshQuotaAdjuster(flags, "refresh-quota-seasonal", -4.4),
                new CapRefreshQuotaAdjuster(8906.0),
                new PercentRefreshQuotaAdjuster(4.0),
                new RefreshQuotaHoursOffsetAdjuster(1.25),
                new CountryRefreshQuotaAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.16, ["online"] = 1.04 }),
                new TieredRefreshQuotaAdjuster(
                    new List<double> { 2867.0, 4589.0, 6252.0, 8380.0 },
                    new List<double> { 1.2, 0.91, 1.07, 1.14 }),
            },
            new List<string>(),
            4.4,
            22516.9);
    }

    public double Run(RefreshQuota item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
