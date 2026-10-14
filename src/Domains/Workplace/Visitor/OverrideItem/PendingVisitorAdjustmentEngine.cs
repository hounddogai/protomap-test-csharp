namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending visitor adjustment calculation.</summary>
public interface IPendingVisitorAdjustmentAdjuster
{
    string Describe();
    double Adjust(PendingVisitorAdjustment item, double valueValue);
}

public sealed record PendingVisitorAdjustmentVersionOffsetAdjuster(double Weight) : IPendingVisitorAdjustmentAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(PendingVisitorAdjustment item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record RegionPendingVisitorAdjustmentAdjuster(Dictionary<string, double> Factors) : IPendingVisitorAdjustmentAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(PendingVisitorAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPendingVisitorAdjustmentAdjuster(double Percent) : IPendingVisitorAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingVisitorAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingVisitorAdjustmentAdjuster(double Cap) : IPendingVisitorAdjustmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingVisitorAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPendingVisitorAdjustmentAdjuster(double Minimum) : IPendingVisitorAdjustmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingVisitorAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pending visitor adjustment steps in order and keeps a trace of each.</summary>
public sealed class PendingVisitorAdjustmentEngine(List<IPendingVisitorAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingVisitorAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingVisitorAdjustmentEngine DefaultPendingVisitorAdjustmentEngine()
    {
        return new PendingVisitorAdjustmentEngine(
            new List<IPendingVisitorAdjustmentAdjuster>
            {
                new PendingVisitorAdjustmentVersionOffsetAdjuster(0.4),
                new RegionPendingVisitorAdjustmentAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.06, ["south"] = 0.93 }),
                new PercentPendingVisitorAdjustmentAdjuster(-17.9),
                new CapPendingVisitorAdjustmentAdjuster(44733.0),
                new MinimumPendingVisitorAdjustmentAdjuster(18.1),
            },
            new List<string>(),
            4.7,
            42810.3);
    }

    public double Evaluate(PendingVisitorAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingVisitorAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
