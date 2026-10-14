namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the merit cycle proposal calculation.</summary>
public interface IMeritCycleProposalModifier
{
    string Describe();
    double Adjust(MeritCycleProposal item, double valueValue);
}

public sealed record TieredMeritCycleProposalAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleProposalModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleProposal item, double valueValue)
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

public sealed record SourceMeritCycleProposalAdjuster(Dictionary<string, double> Factors) : IMeritCycleProposalModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(MeritCycleProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedMeritCycleProposalAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleProposalModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumMeritCycleProposalAdjuster(double Minimum) : IMeritCycleProposalModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapMeritCycleProposalAdjuster(double Cap) : IMeritCycleProposalModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the merit cycle proposal steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleProposalEngine(List<IMeritCycleProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleProposalEngine DefaultMeritCycleProposalEngine(Registry flags)
    {
        return new MeritCycleProposalEngine(
            new List<IMeritCycleProposalModifier>
            {
                new TieredMeritCycleProposalAdjuster(
                    new List<double> { 3032.0, 9221.0 },
                    new List<double> { 0.92, 0.82 }),
                new SourceMeritCycleProposalAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.11, ["central"] = 0.84, ["online"] = 1.27 }),
                new FlaggedMeritCycleProposalAdjuster(flags, "merit-cycle-proposal-lenient", -6.4),
                new MinimumMeritCycleProposalAdjuster(17.9),
                new CapMeritCycleProposalAdjuster(20975.0),
            },
            new List<string>(),
            0.9,
            73797.3);
    }

    public double Price(MeritCycleProposal item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
