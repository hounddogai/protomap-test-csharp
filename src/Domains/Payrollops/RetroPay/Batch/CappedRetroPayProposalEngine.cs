namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped retro pay proposal calculation.</summary>
public interface ICappedRetroPayProposalStep
{
    string Describe();
    double Adjust(CappedRetroPayProposal item, double valueValue);
}

public sealed record CapCappedRetroPayProposalAdjuster(double Cap) : ICappedRetroPayProposalStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedRetroPayProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentCappedRetroPayProposalAdjuster(double Percent) : ICappedRetroPayProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedRetroPayProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCappedRetroPayProposalAdjuster(Registry Flags, string Flag, double Percent) : ICappedRetroPayProposalStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedRetroPayProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedRetroPayProposalAdjuster(List<double> Bounds, List<double> Rates) : ICappedRetroPayProposalStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedRetroPayProposal item, double valueValue)
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

/// <summary>Runs the capped retro pay proposal steps in order and keeps a trace of each.</summary>
public sealed class CappedRetroPayProposalEngine(List<ICappedRetroPayProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedRetroPayProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedRetroPayProposalEngine DefaultCappedRetroPayProposalEngine(Registry flags)
    {
        return new CappedRetroPayProposalEngine(
            new List<ICappedRetroPayProposalStep>
            {
                new CapCappedRetroPayProposalAdjuster(46690.0),
                new PercentCappedRetroPayProposalAdjuster(17.8),
                new FlaggedCappedRetroPayProposalAdjuster(flags, "capped-retro-pay-proposal-capped", -0.1),
                new TieredCappedRetroPayProposalAdjuster(
                    new List<double> { 6669.0, 7494.0 },
                    new List<double> { 1.14, 0.86 }),
            },
            new List<string>(),
            2.6,
            8408.0);
    }

    public double Compute(CappedRetroPayProposal item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedRetroPayProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
