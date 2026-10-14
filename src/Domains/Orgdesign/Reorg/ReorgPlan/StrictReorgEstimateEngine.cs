namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict reorg estimate calculation.</summary>
public interface IStrictReorgEstimateAdjuster
{
    string Describe();
    double Adjust(StrictReorgEstimate item, double valueValue);
}

public sealed record RoundingStrictReorgEstimateAdjuster(double Step) : IStrictReorgEstimateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictReorgEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentStrictReorgEstimateAdjuster(double Percent) : IStrictReorgEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictReorgEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameStrictReorgEstimateAdjuster(Dictionary<string, double> Factors) : IStrictReorgEstimateAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(StrictReorgEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedStrictReorgEstimateAdjuster(Registry Flags, string Flag, double Percent) : IStrictReorgEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictReorgEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapStrictReorgEstimateAdjuster(double Cap) : IStrictReorgEstimateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictReorgEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record StrictReorgEstimateFloorValueOffsetAdjuster(double Weight) : IStrictReorgEstimateAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(StrictReorgEstimate item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

/// <summary>Runs the strict reorg estimate steps in order and keeps a trace of each.</summary>
public sealed class StrictReorgEstimateEngine(List<IStrictReorgEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictReorgEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictReorgEstimateEngine DefaultStrictReorgEstimateEngine(Registry flags)
    {
        return new StrictReorgEstimateEngine(
            new List<IStrictReorgEstimateAdjuster>
            {
                new RoundingStrictReorgEstimateAdjuster(1.0),
                new PercentStrictReorgEstimateAdjuster(-3.1),
                new NameStrictReorgEstimateAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.37, ["south"] = 0.96 }),
                new FlaggedStrictReorgEstimateAdjuster(flags, "strict-reorg-estimate-fallback", 3.8),
                new CapStrictReorgEstimateAdjuster(5482.0),
                new StrictReorgEstimateFloorValueOffsetAdjuster(0.26),
            },
            new List<string>(),
            1.7,
            4286.0);
    }

    public double Run(StrictReorgEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictReorgEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
