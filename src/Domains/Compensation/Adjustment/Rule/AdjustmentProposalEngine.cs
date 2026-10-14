namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment proposal calculation.</summary>
public interface IAdjustmentProposalStep
{
    string Describe();
    double Adjust(AdjustmentProposal item, double valueValue);
}

public sealed record NameAdjustmentProposalAdjuster(Dictionary<string, double> Factors) : IAdjustmentProposalStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdjustmentProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAdjustmentProposalAdjuster(double Minimum) : IAdjustmentProposalStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAdjustmentProposalAdjuster(double Percent) : IAdjustmentProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adjustment proposal steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentProposalEngine(List<IAdjustmentProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentProposalEngine DefaultAdjustmentProposalEngine()
    {
        return new AdjustmentProposalEngine(
            new List<IAdjustmentProposalStep>
            {
                new NameAdjustmentProposalAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.74, ["south"] = 0.8 }),
                new MinimumAdjustmentProposalAdjuster(1.9),
                new PercentAdjustmentProposalAdjuster(15.5),
            },
            new List<string>(),
            0.7,
            94311.0);
    }

    public double Compute(AdjustmentProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
