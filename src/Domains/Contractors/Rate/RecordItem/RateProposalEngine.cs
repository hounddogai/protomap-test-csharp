namespace Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>One step of the rate proposal calculation.</summary>
public interface IRateProposalFactor
{
    string Describe();
    double Adjust(RateProposal item, double valueValue);
}

public sealed record TieredRateProposalAdjuster(List<double> Bounds, List<double> Rates) : IRateProposalFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateProposal item, double valueValue)
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

public sealed record TitleRateProposalAdjuster(Dictionary<string, double> Factors) : IRateProposalFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RateProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRateProposalAdjuster(double Cap) : IRateProposalFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRateProposalAdjuster(double Percent) : IRateProposalFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRateProposalAdjuster(double Minimum) : IRateProposalFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RateProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rate proposal steps in order and keeps a trace of each.</summary>
public sealed class RateProposalEngine(List<IRateProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateProposalEngine DefaultRateProposalEngine()
    {
        return new RateProposalEngine(
            new List<IRateProposalFactor>
            {
                new TieredRateProposalAdjuster(
                    new List<double> { 517.0, 5621.0, 9294.0, 9417.0 },
                    new List<double> { 0.92, 1.14, 1.11, 1.17 }),
                new TitleRateProposalAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.97, ["south"] = 0.94, ["online"] = 1.32 }),
                new CapRateProposalAdjuster(42538.0),
                new PercentRateProposalAdjuster(15.8),
                new MinimumRateProposalAdjuster(35.9),
            },
            new List<string>(),
            4.8,
            93400.2);
    }

    public double Run(RateProposal item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RateProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
