namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction proposal calculation.</summary>
public interface IDeductionProposalAdjuster
{
    string Describe();
    double Adjust(DeductionProposal item, double valueValue);
}

public sealed record CapDeductionProposalAdjuster(double Cap) : IDeductionProposalAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamDeductionProposalAdjuster(Dictionary<string, double> Factors) : IDeductionProposalAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(DeductionProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumDeductionProposalAdjuster(double Minimum) : IDeductionProposalAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeductionProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDeductionProposalAdjuster(Registry Flags, string Flag, double Percent) : IDeductionProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDeductionProposalAdjuster(double Percent) : IDeductionProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDeductionProposalAdjuster(List<double> Bounds, List<double> Rates) : IDeductionProposalAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionProposal item, double valueValue)
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

/// <summary>Runs the deduction proposal steps in order and keeps a trace of each.</summary>
public sealed class DeductionProposalEngine(List<IDeductionProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionProposalEngine DefaultDeductionProposalEngine(Registry flags)
    {
        return new DeductionProposalEngine(
            new List<IDeductionProposalAdjuster>
            {
                new CapDeductionProposalAdjuster(18133.0),
                new TeamDeductionProposalAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.77, ["east"] = 1.17 }),
                new MinimumDeductionProposalAdjuster(18.1),
                new FlaggedDeductionProposalAdjuster(flags, "deduction-proposal-scheduled", -7.7),
                new PercentDeductionProposalAdjuster(15.5),
                new TieredDeductionProposalAdjuster(
                    new List<double> { 4827.0, 5435.0, 6714.0, 8431.0 },
                    new List<double> { 1.11, 1.13, 1.15, 1.03 }),
            },
            new List<string>(),
            4.0,
            84046.9);
    }

    public double Price(DeductionProposal item)
    {
        var current = item.AdjustedValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<DeductionProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
