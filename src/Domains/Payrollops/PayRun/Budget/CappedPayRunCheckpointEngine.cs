namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped pay run checkpoint calculation.</summary>
public interface ICappedPayRunCheckpointModifier
{
    string Describe();
    double Adjust(CappedPayRunCheckpoint item, double valueValue);
}

public sealed record RegionCappedPayRunCheckpointAdjuster(Dictionary<string, double> Factors) : ICappedPayRunCheckpointModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(CappedPayRunCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingCappedPayRunCheckpointAdjuster(double Step) : ICappedPayRunCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedPayRunCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCappedPayRunCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ICappedPayRunCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedPayRunCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedPayRunCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ICappedPayRunCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedPayRunCheckpoint item, double valueValue)
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

public sealed record PercentCappedPayRunCheckpointAdjuster(double Percent) : ICappedPayRunCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedPayRunCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped pay run checkpoint steps in order and keeps a trace of each.</summary>
public sealed class CappedPayRunCheckpointEngine(List<ICappedPayRunCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPayRunCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPayRunCheckpointEngine DefaultCappedPayRunCheckpointEngine(Registry flags)
    {
        return new CappedPayRunCheckpointEngine(
            new List<ICappedPayRunCheckpointModifier>
            {
                new RegionCappedPayRunCheckpointAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.02, ["retail"] = 1.21, ["central"] = 0.76 }),
                new RoundingCappedPayRunCheckpointAdjuster(0.5),
                new FlaggedCappedPayRunCheckpointAdjuster(flags, "capped-pay-run-checkpoint-fallback", 0.4),
                new TieredCappedPayRunCheckpointAdjuster(
                    new List<double> { 516.0, 5372.0 },
                    new List<double> { 1.17, 0.88 }),
                new PercentCappedPayRunCheckpointAdjuster(14.8),
            },
            new List<string>(),
            0.6,
            5602.2);
    }

    public double Run(CappedPayRunCheckpoint item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedPayRunCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
