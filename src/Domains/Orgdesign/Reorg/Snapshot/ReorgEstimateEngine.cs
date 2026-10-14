namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg estimate calculation.</summary>
public interface IReorgEstimateStep
{
    string Describe();
    double Adjust(ReorgEstimate item, double valueValue);
}

public sealed record MinimumReorgEstimateAdjuster(double Minimum) : IReorgEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentReorgEstimateAdjuster(double Percent) : IReorgEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ReorgEstimateEndsAtOffsetAdjuster(double Weight) : IReorgEstimateStep
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(ReorgEstimate item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record FlaggedReorgEstimateAdjuster(Registry Flags, string Flag, double Percent) : IReorgEstimateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapReorgEstimateAdjuster(double Cap) : IReorgEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReorgEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the reorg estimate steps in order and keeps a trace of each.</summary>
public sealed class ReorgEstimateEngine(List<IReorgEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgEstimateEngine DefaultReorgEstimateEngine(Registry flags)
    {
        return new ReorgEstimateEngine(
            new List<IReorgEstimateStep>
            {
                new MinimumReorgEstimateAdjuster(21.3),
                new PercentReorgEstimateAdjuster(22.2),
                new ReorgEstimateEndsAtOffsetAdjuster(1.36),
                new FlaggedReorgEstimateAdjuster(flags, "reorg-estimate-lenient", -0.2),
                new CapReorgEstimateAdjuster(1459.0),
            },
            new List<string>(),
            3.9,
            10751.8);
    }

    public double Compute(ReorgEstimate item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
