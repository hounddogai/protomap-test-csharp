namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor snapshot calculation.</summary>
public interface IVisitorSnapshotStep
{
    string Describe();
    double Adjust(VisitorSnapshot item, double valueValue);
}

public sealed record VisitorSnapshotQuantityOffsetAdjuster(double Weight) : IVisitorSnapshotStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(VisitorSnapshot item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record CapVisitorSnapshotAdjuster(double Cap) : IVisitorSnapshotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelVisitorSnapshotAdjuster(Dictionary<string, double> Factors) : IVisitorSnapshotStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(VisitorSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedVisitorSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IVisitorSnapshotStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumVisitorSnapshotAdjuster(double Minimum) : IVisitorSnapshotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the visitor snapshot steps in order and keeps a trace of each.</summary>
public sealed class VisitorSnapshotEngine(List<IVisitorSnapshotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorSnapshotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorSnapshotEngine DefaultVisitorSnapshotEngine(Registry flags)
    {
        return new VisitorSnapshotEngine(
            new List<IVisitorSnapshotStep>
            {
                new VisitorSnapshotQuantityOffsetAdjuster(1.33),
                new CapVisitorSnapshotAdjuster(14631.0),
                new LabelVisitorSnapshotAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.9, ["central"] = 0.77, ["north"] = 0.97 }),
                new FlaggedVisitorSnapshotAdjuster(flags, "visitor-snapshot-adaptive", 6.5),
                new MinimumVisitorSnapshotAdjuster(48.5),
            },
            new List<string>(),
            4.7,
            43953.6);
    }

    public double Evaluate(VisitorSnapshot item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
