namespace Acme.Workforce.Compensation.EquityGrant.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant assignment calculation.</summary>
public interface IEquityGrantAssignmentAdjuster
{
    string Describe();
    double Adjust(EquityGrantAssignment item, double valueValue);
}

public sealed record PercentEquityGrantAssignmentAdjuster(double Percent) : IEquityGrantAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EquityGrantAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedEquityGrantAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameEquityGrantAssignmentAdjuster(Dictionary<string, double> Factors) : IEquityGrantAssignmentAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(EquityGrantAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the equity grant assignment steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantAssignmentEngine(List<IEquityGrantAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantAssignmentEngine DefaultEquityGrantAssignmentEngine(Registry flags)
    {
        return new EquityGrantAssignmentEngine(
            new List<IEquityGrantAssignmentAdjuster>
            {
                new PercentEquityGrantAssignmentAdjuster(12.3),
                new FlaggedEquityGrantAssignmentAdjuster(flags, "equity-grant-assignment-capped", 9.6),
                new NameEquityGrantAssignmentAdjuster(new Dictionary<string, double> { ["retail"] = 0.8 }),
            },
            new List<string>(),
            1.0,
            94146.6);
    }

    public double Compute(EquityGrantAssignment item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
