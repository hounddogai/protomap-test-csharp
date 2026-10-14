namespace Acme.Workforce.Assets.Assignment.Proposal;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the seasonal assignment proposal calculation.</summary>
public interface ISeasonalAssignmentProposalStep
{
    string Describe();
    double Adjust(SeasonalAssignmentProposal item, double valueValue);
}

public sealed record SeasonalAssignmentProposalDurationMinutesOffsetAdjuster(double Weight) : ISeasonalAssignmentProposalStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(SeasonalAssignmentProposal item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record CountrySeasonalAssignmentProposalAdjuster(Dictionary<string, double> Factors) : ISeasonalAssignmentProposalStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(SeasonalAssignmentProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedSeasonalAssignmentProposalAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalAssignmentProposalStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalAssignmentProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalAssignmentProposalAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalAssignmentProposalStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalAssignmentProposal item, double valueValue)
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

public sealed record MinimumSeasonalAssignmentProposalAdjuster(double Minimum) : ISeasonalAssignmentProposalStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalAssignmentProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSeasonalAssignmentProposalAdjuster(double Percent) : ISeasonalAssignmentProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalAssignmentProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal assignment proposal steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAssignmentProposalEngine(List<ISeasonalAssignmentProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAssignmentProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAssignmentProposalEngine DefaultSeasonalAssignmentProposalEngine(Registry flags)
    {
        return new SeasonalAssignmentProposalEngine(
            new List<ISeasonalAssignmentProposalStep>
            {
                new SeasonalAssignmentProposalDurationMinutesOffsetAdjuster(0.61),
                new CountrySeasonalAssignmentProposalAdjuster(new Dictionary<string, double> { ["retail"] = 1.01 }),
                new FlaggedSeasonalAssignmentProposalAdjuster(flags, "seasonal-assignment-proposal-adaptive", 8.0),
                new TieredSeasonalAssignmentProposalAdjuster(
                    new List<double> { 2693.0, 3064.0, 3355.0 },
                    new List<double> { 0.99, 1.08, 1.03 }),
                new MinimumSeasonalAssignmentProposalAdjuster(36.9),
                new PercentSeasonalAssignmentProposalAdjuster(-7.5),
            },
            new List<string>(),
            4.9,
            53975.6);
    }

    public double Evaluate(SeasonalAssignmentProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAssignmentProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
