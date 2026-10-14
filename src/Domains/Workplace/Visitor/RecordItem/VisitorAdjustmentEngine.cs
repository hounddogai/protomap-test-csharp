namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor adjustment calculation.</summary>
public interface IVisitorAdjustmentStep
{
    string Describe();
    double Adjust(VisitorAdjustment item, double valueValue);
}

public sealed record FlaggedVisitorAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IVisitorAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapVisitorAdjustmentAdjuster(double Cap) : IVisitorAdjustmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record VisitorAdjustmentWindowDaysOffsetAdjuster(double Weight) : IVisitorAdjustmentStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(VisitorAdjustment item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

/// <summary>Runs the visitor adjustment steps in order and keeps a trace of each.</summary>
public sealed class VisitorAdjustmentEngine(List<IVisitorAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorAdjustmentEngine DefaultVisitorAdjustmentEngine(Registry flags)
    {
        return new VisitorAdjustmentEngine(
            new List<IVisitorAdjustmentStep>
            {
                new FlaggedVisitorAdjustmentAdjuster(flags, "visitor-adjustment-global", 9.8),
                new CapVisitorAdjustmentAdjuster(46800.0),
                new VisitorAdjustmentWindowDaysOffsetAdjuster(1.93),
            },
            new List<string>(),
            0.8,
            31052.5);
    }

    public double Compute(VisitorAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<VisitorAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
