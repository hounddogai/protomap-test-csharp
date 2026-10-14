namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional feedback proposal calculation.</summary>
public interface IRegionalFeedbackProposalFactor
{
    string Describe();
    double Adjust(RegionalFeedbackProposal item, double valueValue);
}

public sealed record PercentRegionalFeedbackProposalAdjuster(double Percent) : IRegionalFeedbackProposalFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalFeedbackProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelRegionalFeedbackProposalAdjuster(Dictionary<string, double> Factors) : IRegionalFeedbackProposalFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RegionalFeedbackProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRegionalFeedbackProposalAdjuster(double Minimum) : IRegionalFeedbackProposalFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalFeedbackProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRegionalFeedbackProposalAdjuster(double Step) : IRegionalFeedbackProposalFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalFeedbackProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRegionalFeedbackProposalAdjuster(List<double> Bounds, List<double> Rates) : IRegionalFeedbackProposalFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalFeedbackProposal item, double valueValue)
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

public sealed record RegionalFeedbackProposalLimitOffsetAdjuster(double Weight) : IRegionalFeedbackProposalFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(RegionalFeedbackProposal item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

/// <summary>Runs the regional feedback proposal steps in order and keeps a trace of each.</summary>
public sealed class RegionalFeedbackProposalEngine(List<IRegionalFeedbackProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalFeedbackProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalFeedbackProposalEngine DefaultRegionalFeedbackProposalEngine()
    {
        return new RegionalFeedbackProposalEngine(
            new List<IRegionalFeedbackProposalFactor>
            {
                new PercentRegionalFeedbackProposalAdjuster(-12.0),
                new ChannelRegionalFeedbackProposalAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.8, ["retail"] = 0.89, ["south"] = 0.83 }),
                new MinimumRegionalFeedbackProposalAdjuster(36.1),
                new RoundingRegionalFeedbackProposalAdjuster(0.05),
                new TieredRegionalFeedbackProposalAdjuster(
                    new List<double> { 1011.0, 2319.0, 3842.0, 9723.0 },
                    new List<double> { 0.92, 1.2, 0.84, 1.15 }),
                new RegionalFeedbackProposalLimitOffsetAdjuster(0.02),
            },
            new List<string>(),
            3.2,
            76524.2);
    }

    public double Price(RegionalFeedbackProposal item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalFeedbackProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
