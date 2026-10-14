namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the reorg quota calculation.</summary>
public interface IReorgQuotaAdjuster
{
    string Describe();
    double Adjust(ReorgQuota item, double valueValue);
}

public sealed record MinimumReorgQuotaAdjuster(double Minimum) : IReorgQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentReorgQuotaAdjuster(double Percent) : IReorgQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredReorgQuotaAdjuster(List<double> Bounds, List<double> Rates) : IReorgQuotaAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgQuota item, double valueValue)
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

public sealed record FlaggedReorgQuotaAdjuster(Registry Flags, string Flag, double Percent) : IReorgQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the reorg quota steps in order and keeps a trace of each.</summary>
public sealed class ReorgQuotaEngine(List<IReorgQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgQuotaEngine DefaultReorgQuotaEngine(Registry flags)
    {
        return new ReorgQuotaEngine(
            new List<IReorgQuotaAdjuster>
            {
                new MinimumReorgQuotaAdjuster(11.2),
                new PercentReorgQuotaAdjuster(-14.3),
                new TieredReorgQuotaAdjuster(
                    new List<double> { 1920.0, 6930.0, 7990.0, 8166.0 },
                    new List<double> { 1.08, 1.16, 0.85, 1.1 }),
                new FlaggedReorgQuotaAdjuster(flags, "reorg-quota-strict", -0.8),
            },
            new List<string>(),
            2.1,
            69073.4);
    }

    public double Compute(ReorgQuota item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
