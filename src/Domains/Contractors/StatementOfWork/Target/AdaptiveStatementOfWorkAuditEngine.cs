namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive statement of work audit calculation.</summary>
public interface IAdaptiveStatementOfWorkAuditAdjuster
{
    string Describe();
    double Adjust(AdaptiveStatementOfWorkAudit item, double valueValue);
}

public sealed record ChannelAdaptiveStatementOfWorkAuditAdjuster(Dictionary<string, double> Factors) : IAdaptiveStatementOfWorkAuditAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(AdaptiveStatementOfWorkAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAdaptiveStatementOfWorkAuditAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveStatementOfWorkAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveStatementOfWorkAudit item, double valueValue)
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

public sealed record FlaggedAdaptiveStatementOfWorkAuditAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveStatementOfWorkAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveStatementOfWorkAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdaptiveStatementOfWorkAuditAdjuster(double Percent) : IAdaptiveStatementOfWorkAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveStatementOfWorkAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveStatementOfWorkAuditAdjuster(double Minimum) : IAdaptiveStatementOfWorkAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveStatementOfWorkAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive statement of work audit steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveStatementOfWorkAuditEngine(List<IAdaptiveStatementOfWorkAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveStatementOfWorkAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveStatementOfWorkAuditEngine DefaultAdaptiveStatementOfWorkAuditEngine(Registry flags)
    {
        return new AdaptiveStatementOfWorkAuditEngine(
            new List<IAdaptiveStatementOfWorkAuditAdjuster>
            {
                new ChannelAdaptiveStatementOfWorkAuditAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.15, ["west"] = 1.23, ["south"] = 0.99 }),
                new TieredAdaptiveStatementOfWorkAuditAdjuster(
                    new List<double> { 2134.0, 2149.0 },
                    new List<double> { 1.04, 0.83 }),
                new FlaggedAdaptiveStatementOfWorkAuditAdjuster(
                    flags,
                    "adaptive-statement-of-work-audit-rolling",
                    -6.2),
                new PercentAdaptiveStatementOfWorkAuditAdjuster(-11.9),
                new MinimumAdaptiveStatementOfWorkAuditAdjuster(46.1),
            },
            new List<string>(),
            4.4,
            38548.4);
    }

    public double Price(AdaptiveStatementOfWorkAudit item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveStatementOfWorkAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
