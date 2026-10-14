namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped statement of work proposal calculation.</summary>
public interface ICappedStatementOfWorkProposalModifier
{
    string Describe();
    double Adjust(CappedStatementOfWorkProposal item, double valueValue);
}

public sealed record FlaggedCappedStatementOfWorkProposalAdjuster(Registry Flags, string Flag, double Percent) : ICappedStatementOfWorkProposalModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedStatementOfWorkProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedStatementOfWorkProposalAdjuster(double Step) : ICappedStatementOfWorkProposalModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedStatementOfWorkProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCappedStatementOfWorkProposalAdjuster(double Percent) : ICappedStatementOfWorkProposalModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedStatementOfWorkProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedStatementOfWorkProposalDiscountOffsetAdjuster(double Weight) : ICappedStatementOfWorkProposalModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(CappedStatementOfWorkProposal item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record MinimumCappedStatementOfWorkProposalAdjuster(double Minimum) : ICappedStatementOfWorkProposalModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedStatementOfWorkProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCappedStatementOfWorkProposalAdjuster(List<double> Bounds, List<double> Rates) : ICappedStatementOfWorkProposalModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedStatementOfWorkProposal item, double valueValue)
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

/// <summary>Runs the capped statement of work proposal steps in order and keeps a trace of each.</summary>
public sealed class CappedStatementOfWorkProposalEngine(List<ICappedStatementOfWorkProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedStatementOfWorkProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedStatementOfWorkProposalEngine DefaultCappedStatementOfWorkProposalEngine(Registry flags)
    {
        return new CappedStatementOfWorkProposalEngine(
            new List<ICappedStatementOfWorkProposalModifier>
            {
                new FlaggedCappedStatementOfWorkProposalAdjuster(
                    flags,
                    "capped-statement-of-work-proposal-rolling",
                    -6.2),
                new RoundingCappedStatementOfWorkProposalAdjuster(1.0),
                new PercentCappedStatementOfWorkProposalAdjuster(5.4),
                new CappedStatementOfWorkProposalDiscountOffsetAdjuster(1.63),
                new MinimumCappedStatementOfWorkProposalAdjuster(31.4),
                new TieredCappedStatementOfWorkProposalAdjuster(
                    new List<double> { 6719.0, 6988.0 },
                    new List<double> { 0.93, 1.12 }),
            },
            new List<string>(),
            1.7,
            74577.1);
    }

    public double Evaluate(CappedStatementOfWorkProposal item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedStatementOfWorkProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
