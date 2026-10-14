namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict retro pay proposal calculation.</summary>
public interface IStrictRetroPayProposalAdjuster
{
    string Describe();
    double Adjust(StrictRetroPayProposal item, double valueValue);
}

public sealed record CategoryStrictRetroPayProposalAdjuster(Dictionary<string, double> Factors) : IStrictRetroPayProposalAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(StrictRetroPayProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapStrictRetroPayProposalAdjuster(double Cap) : IStrictRetroPayProposalAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictRetroPayProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentStrictRetroPayProposalAdjuster(double Percent) : IStrictRetroPayProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictRetroPayProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedStrictRetroPayProposalAdjuster(Registry Flags, string Flag, double Percent) : IStrictRetroPayProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictRetroPayProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StrictRetroPayProposalWeightOffsetAdjuster(double Weight) : IStrictRetroPayProposalAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(StrictRetroPayProposal item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record RoundingStrictRetroPayProposalAdjuster(double Step) : IStrictRetroPayProposalAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictRetroPayProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict retro pay proposal steps in order and keeps a trace of each.</summary>
public sealed class StrictRetroPayProposalEngine(List<IStrictRetroPayProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictRetroPayProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictRetroPayProposalEngine DefaultStrictRetroPayProposalEngine(Registry flags)
    {
        return new StrictRetroPayProposalEngine(
            new List<IStrictRetroPayProposalAdjuster>
            {
                new CategoryStrictRetroPayProposalAdjuster(new Dictionary<string, double> { ["west"] = 1.37 }),
                new CapStrictRetroPayProposalAdjuster(33689.0),
                new PercentStrictRetroPayProposalAdjuster(13.9),
                new FlaggedStrictRetroPayProposalAdjuster(flags, "strict-retro-pay-proposal-primary", -6.4),
                new StrictRetroPayProposalWeightOffsetAdjuster(0.91),
                new RoundingStrictRetroPayProposalAdjuster(1.0),
            },
            new List<string>(),
            2.4,
            31911.2);
    }

    public double Price(StrictRetroPayProposal item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StrictRetroPayProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
