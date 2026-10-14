namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive mobility proposal calculation.</summary>
public interface IAdaptiveMobilityProposalStep
{
    string Describe();
    double Adjust(AdaptiveMobilityProposal item, double valueValue);
}

public sealed record RoundingAdaptiveMobilityProposalAdjuster(double Step) : IAdaptiveMobilityProposalStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveMobilityProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAdaptiveMobilityProposalAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveMobilityProposalStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveMobilityProposal item, double valueValue)
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

public sealed record CountryAdaptiveMobilityProposalAdjuster(Dictionary<string, double> Factors) : IAdaptiveMobilityProposalStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(AdaptiveMobilityProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAdaptiveMobilityProposalAdjuster(double Percent) : IAdaptiveMobilityProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveMobilityProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveMobilityProposalAdjuster(double Minimum) : IAdaptiveMobilityProposalStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveMobilityProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdaptiveMobilityProposalAdjuster(double Cap) : IAdaptiveMobilityProposalStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveMobilityProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the adaptive mobility proposal steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveMobilityProposalEngine(List<IAdaptiveMobilityProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveMobilityProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveMobilityProposalEngine DefaultAdaptiveMobilityProposalEngine()
    {
        return new AdaptiveMobilityProposalEngine(
            new List<IAdaptiveMobilityProposalStep>
            {
                new RoundingAdaptiveMobilityProposalAdjuster(0.25),
                new TieredAdaptiveMobilityProposalAdjuster(
                    new List<double> { 3175.0, 4127.0 },
                    new List<double> { 0.84, 0.82 }),
                new CountryAdaptiveMobilityProposalAdjuster(new Dictionary<string, double> { ["south"] = 1.38 }),
                new PercentAdaptiveMobilityProposalAdjuster(1.2),
                new MinimumAdaptiveMobilityProposalAdjuster(30.0),
                new CapAdaptiveMobilityProposalAdjuster(25581.0),
            },
            new List<string>(),
            0.0,
            61431.2);
    }

    public double Compute(AdaptiveMobilityProposal item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveMobilityProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
