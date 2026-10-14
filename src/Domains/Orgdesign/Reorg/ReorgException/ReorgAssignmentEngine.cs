namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg assignment calculation.</summary>
public interface IReorgAssignmentAdjuster
{
    string Describe();
    double Adjust(ReorgAssignment item, double valueValue);
}

public sealed record TitleReorgAssignmentAdjuster(Dictionary<string, double> Factors) : IReorgAssignmentAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ReorgAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedReorgAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IReorgAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingReorgAssignmentAdjuster(double Step) : IReorgAssignmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the reorg assignment steps in order and keeps a trace of each.</summary>
public sealed class ReorgAssignmentEngine(List<IReorgAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgAssignmentEngine DefaultReorgAssignmentEngine(Registry flags)
    {
        return new ReorgAssignmentEngine(
            new List<IReorgAssignmentAdjuster>
            {
                new TitleReorgAssignmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.96, ["east"] = 1.1, ["south"] = 0.97 }),
                new FlaggedReorgAssignmentAdjuster(flags, "reorg-assignment-scheduled", 3.8),
                new RoundingReorgAssignmentAdjuster(0.5),
            },
            new List<string>(),
            0.1,
            47155.0);
    }

    public double Evaluate(ReorgAssignment item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
