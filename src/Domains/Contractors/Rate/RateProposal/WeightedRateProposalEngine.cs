namespace Acme.Workforce.Contractors.Rate.RateProposal;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted rate proposal calculation.</summary>
public interface IWeightedRateProposalAdjuster
{
    string Describe();
    double Adjust(WeightedRateProposal item, double valueValue);
}

public sealed record WeightedRateProposalQuantityOffsetAdjuster(double Weight) : IWeightedRateProposalAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(WeightedRateProposal item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record SourceWeightedRateProposalAdjuster(Dictionary<string, double> Factors) : IWeightedRateProposalAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(WeightedRateProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapWeightedRateProposalAdjuster(double Cap) : IWeightedRateProposalAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedRateProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumWeightedRateProposalAdjuster(double Minimum) : IWeightedRateProposalAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedRateProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedWeightedRateProposalAdjuster(Registry Flags, string Flag, double Percent) : IWeightedRateProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedRateProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedRateProposalAdjuster(List<double> Bounds, List<double> Rates) : IWeightedRateProposalAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedRateProposal item, double valueValue)
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

/// <summary>Runs the weighted rate proposal steps in order and keeps a trace of each.</summary>
public sealed class WeightedRateProposalEngine(List<IWeightedRateProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedRateProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedRateProposalEngine DefaultWeightedRateProposalEngine(Registry flags)
    {
        return new WeightedRateProposalEngine(
            new List<IWeightedRateProposalAdjuster>
            {
                new WeightedRateProposalQuantityOffsetAdjuster(1.4),
                new SourceWeightedRateProposalAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.32, ["central"] = 0.89, ["north"] = 1.09 }),
                new CapWeightedRateProposalAdjuster(24989.0),
                new MinimumWeightedRateProposalAdjuster(34.8),
                new FlaggedWeightedRateProposalAdjuster(flags, "weighted-rate-proposal-tiered", -5.1),
                new TieredWeightedRateProposalAdjuster(
                    new List<double> { 5356.0, 5982.0, 9434.0 },
                    new List<double> { 0.96, 1.08, 0.99 }),
            },
            new List<string>(),
            4.1,
            72700.6);
    }

    public double Price(WeightedRateProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedRateProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
