namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg checkpoint calculation.</summary>
public interface IReorgCheckpointModifier
{
    string Describe();
    double Adjust(ReorgCheckpoint item, double valueValue);
}

public sealed record SourceReorgCheckpointAdjuster(Dictionary<string, double> Factors) : IReorgCheckpointModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(ReorgCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredReorgCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IReorgCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgCheckpoint item, double valueValue)
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

public sealed record PercentReorgCheckpointAdjuster(double Percent) : IReorgCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumReorgCheckpointAdjuster(double Minimum) : IReorgCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingReorgCheckpointAdjuster(double Step) : IReorgCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the reorg checkpoint steps in order and keeps a trace of each.</summary>
public sealed class ReorgCheckpointEngine(List<IReorgCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgCheckpointEngine DefaultReorgCheckpointEngine()
    {
        return new ReorgCheckpointEngine(
            new List<IReorgCheckpointModifier>
            {
                new SourceReorgCheckpointAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.08, ["west"] = 1.03, ["north"] = 1.14 }),
                new TieredReorgCheckpointAdjuster(
                    new List<double> { 735.0, 1568.0, 3973.0, 9687.0 },
                    new List<double> { 1.01, 0.9, 0.83, 1.13 }),
                new PercentReorgCheckpointAdjuster(8.6),
                new MinimumReorgCheckpointAdjuster(14.1),
                new RoundingReorgCheckpointAdjuster(5.0),
            },
            new List<string>(),
            3.2,
            39380.3);
    }

    public double Price(ReorgCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<ReorgCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
