namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict span proposal calculation.</summary>
public interface IStrictSpanProposalFactor
{
    string Describe();
    double Adjust(StrictSpanProposal item, double valueValue);
}

public sealed record RoundingStrictSpanProposalAdjuster(double Step) : IStrictSpanProposalFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictSpanProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredStrictSpanProposalAdjuster(List<double> Bounds, List<double> Rates) : IStrictSpanProposalFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictSpanProposal item, double valueValue)
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

public sealed record FlaggedStrictSpanProposalAdjuster(Registry Flags, string Flag, double Percent) : IStrictSpanProposalFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictSpanProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapStrictSpanProposalAdjuster(double Cap) : IStrictSpanProposalFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictSpanProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumStrictSpanProposalAdjuster(double Minimum) : IStrictSpanProposalFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictSpanProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the strict span proposal steps in order and keeps a trace of each.</summary>
public sealed class StrictSpanProposalEngine(List<IStrictSpanProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictSpanProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictSpanProposalEngine DefaultStrictSpanProposalEngine(Registry flags)
    {
        return new StrictSpanProposalEngine(
            new List<IStrictSpanProposalFactor>
            {
                new RoundingStrictSpanProposalAdjuster(5.0),
                new TieredStrictSpanProposalAdjuster(
                    new List<double> { 1185.0, 1914.0, 7680.0, 9748.0 },
                    new List<double> { 1.16, 0.82, 1.14, 1.06 }),
                new FlaggedStrictSpanProposalAdjuster(flags, "strict-span-proposal-adaptive", 6.1),
                new CapStrictSpanProposalAdjuster(5025.0),
                new MinimumStrictSpanProposalAdjuster(32.6),
            },
            new List<string>(),
            0.3,
            54376.3);
    }

    public double Evaluate(StrictSpanProposal item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictSpanProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
