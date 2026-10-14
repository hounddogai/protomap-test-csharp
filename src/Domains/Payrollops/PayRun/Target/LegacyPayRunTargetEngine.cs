namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy pay run target calculation.</summary>
public interface ILegacyPayRunTargetStep
{
    string Describe();
    double Adjust(LegacyPayRunTarget item, double valueValue);
}

public sealed record LegacyPayRunTargetRatioOffsetAdjuster(double Weight) : ILegacyPayRunTargetStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(LegacyPayRunTarget item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record FlaggedLegacyPayRunTargetAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPayRunTargetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPayRunTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitLegacyPayRunTargetAdjuster(Dictionary<string, double> Factors) : ILegacyPayRunTargetStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(LegacyPayRunTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLegacyPayRunTargetAdjuster(double Percent) : ILegacyPayRunTargetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyPayRunTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy pay run target steps in order and keeps a trace of each.</summary>
public sealed class LegacyPayRunTargetEngine(List<ILegacyPayRunTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPayRunTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPayRunTargetEngine DefaultLegacyPayRunTargetEngine(Registry flags)
    {
        return new LegacyPayRunTargetEngine(
            new List<ILegacyPayRunTargetStep>
            {
                new LegacyPayRunTargetRatioOffsetAdjuster(1.78),
                new FlaggedLegacyPayRunTargetAdjuster(flags, "legacy-pay-run-target-scheduled", 0.5),
                new UnitLegacyPayRunTargetAdjuster(new Dictionary<string, double> { ["south"] = 1.17 }),
                new PercentLegacyPayRunTargetAdjuster(19.8),
            },
            new List<string>(),
            3.1,
            95915.8);
    }

    public double Price(LegacyPayRunTarget item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPayRunTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
