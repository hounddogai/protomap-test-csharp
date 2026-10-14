namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive headcount audit calculation.</summary>
public interface IAdaptiveHeadcountAuditAdjuster
{
    string Describe();
    double Adjust(AdaptiveHeadcountAudit item, double valueValue);
}

public sealed record PercentAdaptiveHeadcountAuditAdjuster(double Percent) : IAdaptiveHeadcountAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveHeadcountAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveHeadcountAuditAdjuster(double Minimum) : IAdaptiveHeadcountAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveHeadcountAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdaptiveHeadcountAuditAdjuster(double Cap) : IAdaptiveHeadcountAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveHeadcountAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAdaptiveHeadcountAuditAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveHeadcountAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveHeadcountAudit item, double valueValue)
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

public sealed record RoundingAdaptiveHeadcountAuditAdjuster(double Step) : IAdaptiveHeadcountAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveHeadcountAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAdaptiveHeadcountAuditAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveHeadcountAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveHeadcountAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive headcount audit steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveHeadcountAuditEngine(List<IAdaptiveHeadcountAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveHeadcountAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveHeadcountAuditEngine DefaultAdaptiveHeadcountAuditEngine(Registry flags)
    {
        return new AdaptiveHeadcountAuditEngine(
            new List<IAdaptiveHeadcountAuditAdjuster>
            {
                new PercentAdaptiveHeadcountAuditAdjuster(-7.8),
                new MinimumAdaptiveHeadcountAuditAdjuster(0.9),
                new CapAdaptiveHeadcountAuditAdjuster(30086.0),
                new TieredAdaptiveHeadcountAuditAdjuster(
                    new List<double> { 6980.0, 8533.0 },
                    new List<double> { 1.13, 0.94 }),
                new RoundingAdaptiveHeadcountAuditAdjuster(0.05),
                new FlaggedAdaptiveHeadcountAuditAdjuster(flags, "adaptive-headcount-audit-batched", -8.0),
            },
            new List<string>(),
            3.1,
            35511.2);
    }

    public double Compute(AdaptiveHeadcountAudit item)
    {
        var current = item.NetLoad();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<AdaptiveHeadcountAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
