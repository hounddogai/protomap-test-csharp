namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending exception proposal calculation.</summary>
public interface IPendingExceptionProposalStep
{
    string Describe();
    double Adjust(PendingExceptionProposal item, double valueValue);
}

public sealed record TitlePendingExceptionProposalAdjuster(Dictionary<string, double> Factors) : IPendingExceptionProposalStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PendingExceptionProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPendingExceptionProposalAdjuster(double Step) : IPendingExceptionProposalStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingExceptionProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPendingExceptionProposalAdjuster(double Minimum) : IPendingExceptionProposalStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingExceptionProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPendingExceptionProposalAdjuster(double Cap) : IPendingExceptionProposalStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingExceptionProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pending exception proposal steps in order and keeps a trace of each.</summary>
public sealed class PendingExceptionProposalEngine(List<IPendingExceptionProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingExceptionProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingExceptionProposalEngine DefaultPendingExceptionProposalEngine()
    {
        return new PendingExceptionProposalEngine(
            new List<IPendingExceptionProposalStep>
            {
                new TitlePendingExceptionProposalAdjuster(new Dictionary<string, double> { ["north"] = 1.34 }),
                new RoundingPendingExceptionProposalAdjuster(0.5),
                new MinimumPendingExceptionProposalAdjuster(13.3),
                new CapPendingExceptionProposalAdjuster(35434.0),
            },
            new List<string>(),
            0.3,
            40035.1);
    }

    public double Price(PendingExceptionProposal item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PendingExceptionProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
