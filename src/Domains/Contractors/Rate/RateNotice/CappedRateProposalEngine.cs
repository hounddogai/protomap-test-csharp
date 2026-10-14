namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped rate proposal calculation.</summary>
public interface ICappedRateProposalStep
{
    string Describe();
    double Adjust(CappedRateProposal item, double valueValue);
}

public sealed record NameCappedRateProposalAdjuster(Dictionary<string, double> Factors) : ICappedRateProposalStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(CappedRateProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredCappedRateProposalAdjuster(List<double> Bounds, List<double> Rates) : ICappedRateProposalStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedRateProposal item, double valueValue)
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

public sealed record RoundingCappedRateProposalAdjuster(double Step) : ICappedRateProposalStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedRateProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCappedRateProposalAdjuster(double Percent) : ICappedRateProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedRateProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCappedRateProposalAdjuster(Registry Flags, string Flag, double Percent) : ICappedRateProposalStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedRateProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped rate proposal steps in order and keeps a trace of each.</summary>
public sealed class CappedRateProposalEngine(List<ICappedRateProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedRateProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedRateProposalEngine DefaultCappedRateProposalEngine(Registry flags)
    {
        return new CappedRateProposalEngine(
            new List<ICappedRateProposalStep>
            {
                new NameCappedRateProposalAdjuster(new Dictionary<string, double> { ["south"] = 1.07 }),
                new TieredCappedRateProposalAdjuster(
                    new List<double> { 2030.0, 3811.0, 5971.0, 7655.0 },
                    new List<double> { 0.86, 0.81, 0.94, 1.02 }),
                new RoundingCappedRateProposalAdjuster(1.0),
                new PercentCappedRateProposalAdjuster(-15.4),
                new FlaggedCappedRateProposalAdjuster(flags, "capped-rate-proposal-manual", 5.7),
            },
            new List<string>(),
            2.7,
            90377.6);
    }

    public double Run(CappedRateProposal item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CappedRateProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
