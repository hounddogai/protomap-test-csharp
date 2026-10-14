namespace Acme.Workforce.Orgdesign.Position.PositionAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position proposal calculation.</summary>
public interface IPositionProposalFactor
{
    string Describe();
    double Adjust(PositionProposal item, double valueValue);
}

public sealed record SegmentPositionProposalAdjuster(Dictionary<string, double> Factors) : IPositionProposalFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PositionProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPositionProposalAdjuster(double Step) : IPositionProposalFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PositionProposalMarginOffsetAdjuster(double Weight) : IPositionProposalFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(PositionProposal item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record CapPositionProposalAdjuster(double Cap) : IPositionProposalFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the position proposal steps in order and keeps a trace of each.</summary>
public sealed class PositionProposalEngine(List<IPositionProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionProposalEngine DefaultPositionProposalEngine()
    {
        return new PositionProposalEngine(
            new List<IPositionProposalFactor>
            {
                new SegmentPositionProposalAdjuster(new Dictionary<string, double> { ["west"] = 0.87 }),
                new RoundingPositionProposalAdjuster(0.05),
                new PositionProposalMarginOffsetAdjuster(1.4),
                new CapPositionProposalAdjuster(7010.0),
            },
            new List<string>(),
            0.0,
            61038.8);
    }

    public double Evaluate(PositionProposal item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PositionProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
