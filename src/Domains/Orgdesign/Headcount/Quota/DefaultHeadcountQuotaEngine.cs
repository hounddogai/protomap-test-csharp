namespace Acme.Workforce.Orgdesign.Headcount.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default headcount quota calculation.</summary>
public interface IDefaultHeadcountQuotaStep
{
    string Describe();
    double Adjust(DefaultHeadcountQuota item, double valueValue);
}

public sealed record CapDefaultHeadcountQuotaAdjuster(double Cap) : IDefaultHeadcountQuotaStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultHeadcountQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultHeadcountQuotaAdjuster(Registry Flags, string Flag, double Percent) : IDefaultHeadcountQuotaStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultHeadcountQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultHeadcountQuotaAdjuster(List<double> Bounds, List<double> Rates) : IDefaultHeadcountQuotaStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultHeadcountQuota item, double valueValue)
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

public sealed record RoundingDefaultHeadcountQuotaAdjuster(double Step) : IDefaultHeadcountQuotaStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultHeadcountQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default headcount quota steps in order and keeps a trace of each.</summary>
public sealed class DefaultHeadcountQuotaEngine(List<IDefaultHeadcountQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultHeadcountQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultHeadcountQuotaEngine DefaultDefaultHeadcountQuotaEngine(Registry flags)
    {
        return new DefaultHeadcountQuotaEngine(
            new List<IDefaultHeadcountQuotaStep>
            {
                new CapDefaultHeadcountQuotaAdjuster(36922.0),
                new FlaggedDefaultHeadcountQuotaAdjuster(flags, "default-headcount-quota-weighted", 3.5),
                new TieredDefaultHeadcountQuotaAdjuster(
                    new List<double> { 1476.0, 2473.0, 3142.0 },
                    new List<double> { 1.17, 1.11, 1.13 }),
                new RoundingDefaultHeadcountQuotaAdjuster(1.0),
            },
            new List<string>(),
            0.1,
            25008.3);
    }

    public double Price(DefaultHeadcountQuota item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultHeadcountQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
