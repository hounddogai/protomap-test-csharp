namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy visitor checkpoint calculation.</summary>
public interface ILegacyVisitorCheckpointModifier
{
    string Describe();
    double Adjust(LegacyVisitorCheckpoint item, double valueValue);
}

public sealed record TieredLegacyVisitorCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ILegacyVisitorCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyVisitorCheckpoint item, double valueValue)
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

public sealed record CapLegacyVisitorCheckpointAdjuster(double Cap) : ILegacyVisitorCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyVisitorCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLegacyVisitorCheckpointAdjuster(double Percent) : ILegacyVisitorCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyVisitorCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentLegacyVisitorCheckpointAdjuster(Dictionary<string, double> Factors) : ILegacyVisitorCheckpointModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(LegacyVisitorCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLegacyVisitorCheckpointAdjuster(double Step) : ILegacyVisitorCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyVisitorCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy visitor checkpoint steps in order and keeps a trace of each.</summary>
public sealed class LegacyVisitorCheckpointEngine(List<ILegacyVisitorCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyVisitorCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyVisitorCheckpointEngine DefaultLegacyVisitorCheckpointEngine()
    {
        return new LegacyVisitorCheckpointEngine(
            new List<ILegacyVisitorCheckpointModifier>
            {
                new TieredLegacyVisitorCheckpointAdjuster(
                    new List<double> { 4005.0, 4567.0 },
                    new List<double> { 0.81, 1.0 }),
                new CapLegacyVisitorCheckpointAdjuster(10531.0),
                new PercentLegacyVisitorCheckpointAdjuster(-12.6),
                new DepartmentLegacyVisitorCheckpointAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.35, ["retail"] = 1.12, ["east"] = 1.01 }),
                new RoundingLegacyVisitorCheckpointAdjuster(1.0),
            },
            new List<string>(),
            4.4,
            11035.0);
    }

    public double Price(LegacyVisitorCheckpoint item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyVisitorCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
