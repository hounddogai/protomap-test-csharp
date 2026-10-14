namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict desk proposal calculation.</summary>
public interface IStrictDeskProposalStep
{
    string Describe();
    double Adjust(StrictDeskProposal item, double valueValue);
}

public sealed record TieredStrictDeskProposalAdjuster(List<double> Bounds, List<double> Rates) : IStrictDeskProposalStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictDeskProposal item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record SegmentStrictDeskProposalAdjuster(Dictionary<string, double> Factors) : IStrictDeskProposalStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(StrictDeskProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentStrictDeskProposalAdjuster(double Percent) : IStrictDeskProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictDeskProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStrictDeskProposalAdjuster(double Minimum) : IStrictDeskProposalStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictDeskProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingStrictDeskProposalAdjuster(double Step) : IStrictDeskProposalStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictDeskProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StrictDeskProposalWindowDaysOffsetAdjuster(double Weight) : IStrictDeskProposalStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(StrictDeskProposal item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

/// <summary>Runs the strict desk proposal steps in order and keeps a trace of each.</summary>
public sealed class StrictDeskProposalEngine(List<IStrictDeskProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictDeskProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictDeskProposalEngine DefaultStrictDeskProposalEngine()
    {
        return new StrictDeskProposalEngine(
            new List<IStrictDeskProposalStep>
            {
                new TieredStrictDeskProposalAdjuster(
                    new List<double> { 1747.0, 3379.0 },
                    new List<double> { 1.03, 1.01 }),
                new SegmentStrictDeskProposalAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.32, ["central"] = 0.72, ["west"] = 0.95 }),
                new PercentStrictDeskProposalAdjuster(-16.8),
                new MinimumStrictDeskProposalAdjuster(3.3),
                new RoundingStrictDeskProposalAdjuster(5.0),
                new StrictDeskProposalWindowDaysOffsetAdjuster(1.32),
            },
            new List<string>(),
            1.0,
            98034.9);
    }

    public double Compute(StrictDeskProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictDeskProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
