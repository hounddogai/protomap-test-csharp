namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the succession plan proposal calculation.</summary>
public interface ISuccessionPlanProposalStep
{
    string Describe();
    double Adjust(SuccessionPlanProposal item, double valueValue);
}

public sealed record SuccessionPlanProposalHoursOffsetAdjuster(double Weight) : ISuccessionPlanProposalStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(SuccessionPlanProposal item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record MinimumSuccessionPlanProposalAdjuster(double Minimum) : ISuccessionPlanProposalStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameSuccessionPlanProposalAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanProposalStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SuccessionPlanProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSuccessionPlanProposalAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanProposalStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanProposal item, double valueValue)
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

public sealed record FlaggedSuccessionPlanProposalAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanProposalStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSuccessionPlanProposalAdjuster(double Percent) : ISuccessionPlanProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the succession plan proposal steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanProposalEngine(List<ISuccessionPlanProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanProposalEngine DefaultSuccessionPlanProposalEngine(Registry flags)
    {
        return new SuccessionPlanProposalEngine(
            new List<ISuccessionPlanProposalStep>
            {
                new SuccessionPlanProposalHoursOffsetAdjuster(0.86),
                new MinimumSuccessionPlanProposalAdjuster(15.7),
                new NameSuccessionPlanProposalAdjuster(new Dictionary<string, double> { ["east"] = 0.8 }),
                new TieredSuccessionPlanProposalAdjuster(
                    new List<double> { 2399.0, 3269.0 },
                    new List<double> { 0.92, 0.87 }),
                new FlaggedSuccessionPlanProposalAdjuster(flags, "succession-plan-proposal-rolling", 3.2),
                new PercentSuccessionPlanProposalAdjuster(-13.7),
            },
            new List<string>(),
            3.6,
            52517.0);
    }

    public double Run(SuccessionPlanProposal item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
