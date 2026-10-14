namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the nomination assignment calculation.</summary>
public interface INominationAssignmentModifier
{
    string Describe();
    double Adjust(NominationAssignment item, double valueValue);
}

public sealed record FlaggedNominationAssignmentAdjuster(Registry Flags, string Flag, double Percent) : INominationAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredNominationAssignmentAdjuster(List<double> Bounds, List<double> Rates) : INominationAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationAssignment item, double valueValue)
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

public sealed record NominationAssignmentHoursOffsetAdjuster(double Weight) : INominationAssignmentModifier
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(NominationAssignment item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the nomination assignment steps in order and keeps a trace of each.</summary>
public sealed class NominationAssignmentEngine(List<INominationAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationAssignmentEngine DefaultNominationAssignmentEngine(Registry flags)
    {
        return new NominationAssignmentEngine(
            new List<INominationAssignmentModifier>
            {
                new FlaggedNominationAssignmentAdjuster(flags, "nomination-assignment-weighted", 2.5),
                new TieredNominationAssignmentAdjuster(
                    new List<double> { 249.0, 8747.0 },
                    new List<double> { 1.02, 1.01 }),
                new NominationAssignmentHoursOffsetAdjuster(1.24),
            },
            new List<string>(),
            0.9,
            99165.6);
    }

    public double Compute(NominationAssignment item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<NominationAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
