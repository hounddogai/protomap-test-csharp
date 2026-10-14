namespace Acme.Workforce.Absence.Balance.Checkpoint;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy balance adjustment calculation.</summary>
public interface ILegacyBalanceAdjustmentStep
{
    string Describe();
    double Adjust(LegacyBalanceAdjustment item, double valueValue);
}

public sealed record MinimumLegacyBalanceAdjustmentAdjuster(double Minimum) : ILegacyBalanceAdjustmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyBalanceAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLegacyBalanceAdjustmentAdjuster(double Percent) : ILegacyBalanceAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyBalanceAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLegacyBalanceAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : ILegacyBalanceAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyBalanceAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy balance adjustment steps in order and keeps a trace of each.</summary>
public sealed class LegacyBalanceAdjustmentEngine(List<ILegacyBalanceAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyBalanceAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyBalanceAdjustmentEngine DefaultLegacyBalanceAdjustmentEngine(Registry flags)
    {
        return new LegacyBalanceAdjustmentEngine(
            new List<ILegacyBalanceAdjustmentStep>
            {
                new MinimumLegacyBalanceAdjustmentAdjuster(45.3),
                new PercentLegacyBalanceAdjustmentAdjuster(18.3),
                new FlaggedLegacyBalanceAdjustmentAdjuster(flags, "legacy-balance-adjustment-manual", 2.0),
            },
            new List<string>(),
            0.7,
            70391.7);
    }

    public double Price(LegacyBalanceAdjustment item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyBalanceAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
