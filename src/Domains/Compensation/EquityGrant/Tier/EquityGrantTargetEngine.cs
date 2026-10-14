namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant target calculation.</summary>
public interface IEquityGrantTargetFactor
{
    string Describe();
    double Adjust(EquityGrantTarget item, double valueValue);
}

public sealed record CapEquityGrantTargetAdjuster(double Cap) : IEquityGrantTargetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedEquityGrantTargetAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantTargetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record EquityGrantTargetBatchSizeOffsetAdjuster(double Weight) : IEquityGrantTargetFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(EquityGrantTarget item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record RoundingEquityGrantTargetAdjuster(double Step) : IEquityGrantTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the equity grant target steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantTargetEngine(List<IEquityGrantTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantTargetEngine DefaultEquityGrantTargetEngine(Registry flags)
    {
        return new EquityGrantTargetEngine(
            new List<IEquityGrantTargetFactor>
            {
                new CapEquityGrantTargetAdjuster(24153.0),
                new FlaggedEquityGrantTargetAdjuster(flags, "equity-grant-target-batched", -9.7),
                new EquityGrantTargetBatchSizeOffsetAdjuster(1.76),
                new RoundingEquityGrantTargetAdjuster(0.05),
            },
            new List<string>(),
            3.3,
            20973.9);
    }

    public double Run(EquityGrantTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
