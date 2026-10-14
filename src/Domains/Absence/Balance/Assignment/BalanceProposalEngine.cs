namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance proposal calculation.</summary>
public interface IBalanceProposalFactor
{
    string Describe();
    double Adjust(BalanceProposal item, double valueValue);
}

public sealed record CapBalanceProposalAdjuster(double Cap) : IBalanceProposalFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BalanceProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBalanceProposalAdjuster(double Step) : IBalanceProposalFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BalanceProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBalanceProposalAdjuster(double Percent) : IBalanceProposalFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBalanceProposalAdjuster(double Minimum) : IBalanceProposalFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the balance proposal steps in order and keeps a trace of each.</summary>
public sealed class BalanceProposalEngine(List<IBalanceProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceProposalEngine DefaultBalanceProposalEngine()
    {
        return new BalanceProposalEngine(
            new List<IBalanceProposalFactor>
            {
                new CapBalanceProposalAdjuster(12581.0),
                new RoundingBalanceProposalAdjuster(0.25),
                new PercentBalanceProposalAdjuster(-19.4),
                new MinimumBalanceProposalAdjuster(48.1),
            },
            new List<string>(),
            1.0,
            90709.7);
    }

    public double Run(BalanceProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<BalanceProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
