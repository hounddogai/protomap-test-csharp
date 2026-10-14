namespace Acme.Workforce.Compensation.Adjustment.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback adjustment proposal calculation.</summary>
public interface IFallbackAdjustmentProposalStep
{
    string Describe();
    double Adjust(FallbackAdjustmentProposal item, double valueValue);
}

public sealed record RoundingFallbackAdjustmentProposalAdjuster(double Step) : IFallbackAdjustmentProposalStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackAdjustmentProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackAdjustmentProposalAdjuster(double Percent) : IFallbackAdjustmentProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackAdjustmentProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentFallbackAdjustmentProposalAdjuster(Dictionary<string, double> Factors) : IFallbackAdjustmentProposalStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(FallbackAdjustmentProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the fallback adjustment proposal steps in order and keeps a trace of each.</summary>
public sealed class FallbackAdjustmentProposalEngine(List<IFallbackAdjustmentProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackAdjustmentProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackAdjustmentProposalEngine DefaultFallbackAdjustmentProposalEngine()
    {
        return new FallbackAdjustmentProposalEngine(
            new List<IFallbackAdjustmentProposalStep>
            {
                new RoundingFallbackAdjustmentProposalAdjuster(1.0),
                new PercentFallbackAdjustmentProposalAdjuster(-9.2),
                new SegmentFallbackAdjustmentProposalAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.31, ["east"] = 0.8 }),
            },
            new List<string>(),
            2.5,
            26622.5);
    }

    public double Price(FallbackAdjustmentProposal item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackAdjustmentProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
