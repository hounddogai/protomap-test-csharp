namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the assignment entry calculation.</summary>
public interface IAssignmentEntryFactor
{
    string Describe();
    double Adjust(AssignmentEntry item, double valueValue);
}

public sealed record FlaggedAssignmentEntryAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentEntryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAssignmentEntryAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentEntryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentEntry item, double valueValue)
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

public sealed record AssignmentEntryCostOffsetAdjuster(double Weight) : IAssignmentEntryFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(AssignmentEntry item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record MinimumAssignmentEntryAdjuster(double Minimum) : IAssignmentEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssignmentEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAssignmentEntryAdjuster(double Percent) : IAssignmentEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssignmentEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitAssignmentEntryAdjuster(Dictionary<string, double> Factors) : IAssignmentEntryFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AssignmentEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the assignment entry steps in order and keeps a trace of each.</summary>
public sealed class AssignmentEntryEngine(List<IAssignmentEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentEntryEngine DefaultAssignmentEntryEngine(Registry flags)
    {
        return new AssignmentEntryEngine(
            new List<IAssignmentEntryFactor>
            {
                new FlaggedAssignmentEntryAdjuster(flags, "assignment-entry-capped", -1.7),
                new TieredAssignmentEntryAdjuster(
                    new List<double> { 2541.0, 3579.0 },
                    new List<double> { 1.13, 0.86 }),
                new AssignmentEntryCostOffsetAdjuster(0.7),
                new MinimumAssignmentEntryAdjuster(11.5),
                new PercentAssignmentEntryAdjuster(1.0),
                new UnitAssignmentEntryAdjuster(new Dictionary<string, double> { ["south"] = 1.05 }),
            },
            new List<string>(),
            1.7,
            24202.5);
    }

    public double Run(AssignmentEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
