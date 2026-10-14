namespace Acme.Workforce.Contractors.Engagement.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global engagement proposal calculation.</summary>
public interface IGlobalEngagementProposalAdjuster
{
    string Describe();
    double Adjust(GlobalEngagementProposal item, double valueValue);
}

public sealed record TieredGlobalEngagementProposalAdjuster(List<double> Bounds, List<double> Rates) : IGlobalEngagementProposalAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalEngagementProposal item, double valueValue)
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

public sealed record MinimumGlobalEngagementProposalAdjuster(double Minimum) : IGlobalEngagementProposalAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalEngagementProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record GlobalEngagementProposalRatingOffsetAdjuster(double Weight) : IGlobalEngagementProposalAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(GlobalEngagementProposal item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record PercentGlobalEngagementProposalAdjuster(double Percent) : IGlobalEngagementProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalEngagementProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingGlobalEngagementProposalAdjuster(double Step) : IGlobalEngagementProposalAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalEngagementProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapGlobalEngagementProposalAdjuster(double Cap) : IGlobalEngagementProposalAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalEngagementProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the global engagement proposal steps in order and keeps a trace of each.</summary>
public sealed class GlobalEngagementProposalEngine(List<IGlobalEngagementProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalEngagementProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalEngagementProposalEngine DefaultGlobalEngagementProposalEngine()
    {
        return new GlobalEngagementProposalEngine(
            new List<IGlobalEngagementProposalAdjuster>
            {
                new TieredGlobalEngagementProposalAdjuster(
                    new List<double> { 3752.0, 5306.0, 5388.0 },
                    new List<double> { 1.1, 0.81, 1.19 }),
                new MinimumGlobalEngagementProposalAdjuster(5.9),
                new GlobalEngagementProposalRatingOffsetAdjuster(0.21),
                new PercentGlobalEngagementProposalAdjuster(-7.3),
                new RoundingGlobalEngagementProposalAdjuster(0.5),
                new CapGlobalEngagementProposalAdjuster(38945.0),
            },
            new List<string>(),
            4.9,
            29143.7);
    }

    public double Price(GlobalEngagementProposal item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalEngagementProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
