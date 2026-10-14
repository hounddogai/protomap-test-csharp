namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay run proposal calculation.</summary>
public interface IPayRunProposalModifier
{
    string Describe();
    double Adjust(PayRunProposal item, double valueValue);
}

public sealed record PercentPayRunProposalAdjuster(double Percent) : IPayRunProposalModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPayRunProposalAdjuster(Registry Flags, string Flag, double Percent) : IPayRunProposalModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayRunProposalAdjuster(List<double> Bounds, List<double> Rates) : IPayRunProposalModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunProposal item, double valueValue)
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

/// <summary>Runs the pay run proposal steps in order and keeps a trace of each.</summary>
public sealed class PayRunProposalEngine(List<IPayRunProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunProposalEngine DefaultPayRunProposalEngine(Registry flags)
    {
        return new PayRunProposalEngine(
            new List<IPayRunProposalModifier>
            {
                new PercentPayRunProposalAdjuster(8.5),
                new FlaggedPayRunProposalAdjuster(flags, "pay-run-proposal-seasonal", 9.5),
                new TieredPayRunProposalAdjuster(
                    new List<double> { 1292.0, 2076.0, 2709.0, 2943.0 },
                    new List<double> { 1.1, 0.9, 0.83, 1.02 }),
            },
            new List<string>(),
            2.1,
            53372.4);
    }

    public double Run(PayRunProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<PayRunProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
