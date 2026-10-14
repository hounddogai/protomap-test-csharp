namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay band proposal calculation.</summary>
public interface IPayBandProposalStep
{
    string Describe();
    double Adjust(PayBandProposal item, double valueValue);
}

public sealed record PercentPayBandProposalAdjuster(double Percent) : IPayBandProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayBandProposalAdjuster(double Cap) : IPayBandProposalStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPayBandProposalAdjuster(List<double> Bounds, List<double> Rates) : IPayBandProposalStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandProposal item, double valueValue)
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

public sealed record FlaggedPayBandProposalAdjuster(Registry Flags, string Flag, double Percent) : IPayBandProposalStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitlePayBandProposalAdjuster(Dictionary<string, double> Factors) : IPayBandProposalStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PayBandProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay band proposal steps in order and keeps a trace of each.</summary>
public sealed class PayBandProposalEngine(List<IPayBandProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandProposalEngine DefaultPayBandProposalEngine(Registry flags)
    {
        return new PayBandProposalEngine(
            new List<IPayBandProposalStep>
            {
                new PercentPayBandProposalAdjuster(-8.1),
                new CapPayBandProposalAdjuster(33723.0),
                new TieredPayBandProposalAdjuster(new List<double> { 378.0, 7429.0 }, new List<double> { 1.08, 1.0 }),
                new FlaggedPayBandProposalAdjuster(flags, "pay-band-proposal-regional", -3.4),
                new TitlePayBandProposalAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.95, ["south"] = 1.19 }),
            },
            new List<string>(),
            4.7,
            28087.8);
    }

    public double Compute(PayBandProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<PayBandProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
