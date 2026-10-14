namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work proposal calculation.</summary>
public interface IStatementOfWorkProposalStep
{
    string Describe();
    double Adjust(StatementOfWorkProposal item, double valueValue);
}

public sealed record PercentStatementOfWorkProposalAdjuster(double Percent) : IStatementOfWorkProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedStatementOfWorkProposalAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkProposalStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStatementOfWorkProposalAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkProposalStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkProposal item, double valueValue)
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

public sealed record StatementOfWorkProposalStartsAtOffsetAdjuster(double Weight) : IStatementOfWorkProposalStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(StatementOfWorkProposal item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

/// <summary>Runs the statement of work proposal steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkProposalEngine(List<IStatementOfWorkProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkProposalEngine DefaultStatementOfWorkProposalEngine(Registry flags)
    {
        return new StatementOfWorkProposalEngine(
            new List<IStatementOfWorkProposalStep>
            {
                new PercentStatementOfWorkProposalAdjuster(-2.5),
                new FlaggedStatementOfWorkProposalAdjuster(flags, "statement-of-work-proposal-primary", -0.3),
                new TieredStatementOfWorkProposalAdjuster(
                    new List<double> { 1695.0, 3344.0, 4019.0 },
                    new List<double> { 1.19, 0.83, 1.13 }),
                new StatementOfWorkProposalStartsAtOffsetAdjuster(1.54),
            },
            new List<string>(),
            3.3,
            64797.1);
    }

    public double Compute(StatementOfWorkProposal item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
