namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the engagement proposal calculation.</summary>
public interface IEngagementProposalAdjuster
{
    string Describe();
    double Adjust(EngagementProposal item, double valueValue);
}

public sealed record MinimumEngagementProposalAdjuster(double Minimum) : IEngagementProposalAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredEngagementProposalAdjuster(List<double> Bounds, List<double> Rates) : IEngagementProposalAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementProposal item, double valueValue)
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

public sealed record FlaggedEngagementProposalAdjuster(Registry Flags, string Flag, double Percent) : IEngagementProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapEngagementProposalAdjuster(double Cap) : IEngagementProposalAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameEngagementProposalAdjuster(Dictionary<string, double> Factors) : IEngagementProposalAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(EngagementProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the engagement proposal steps in order and keeps a trace of each.</summary>
public sealed class EngagementProposalEngine(List<IEngagementProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementProposalEngine DefaultEngagementProposalEngine(Registry flags)
    {
        return new EngagementProposalEngine(
            new List<IEngagementProposalAdjuster>
            {
                new MinimumEngagementProposalAdjuster(19.8),
                new TieredEngagementProposalAdjuster(
                    new List<double> { 357.0, 2046.0, 4909.0, 7278.0 },
                    new List<double> { 0.83, 1.17, 0.92, 1.06 }),
                new FlaggedEngagementProposalAdjuster(flags, "engagement-proposal-seasonal", 7.6),
                new CapEngagementProposalAdjuster(35988.0),
                new NameEngagementProposalAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.8, ["retail"] = 0.76, ["south"] = 1.28 }),
            },
            new List<string>(),
            1.8,
            22544.4);
    }

    public double Run(EngagementProposal item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<EngagementProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
