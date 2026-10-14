namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the merit cycle entry calculation.</summary>
public interface IMeritCycleEntryStep
{
    string Describe();
    double Adjust(MeritCycleEntry item, double valueValue);
}

public sealed record MeritCycleEntryRevisionOffsetAdjuster(double Weight) : IMeritCycleEntryStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(MeritCycleEntry item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record TieredMeritCycleEntryAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleEntryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleEntry item, double valueValue)
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

public sealed record TitleMeritCycleEntryAdjuster(Dictionary<string, double> Factors) : IMeritCycleEntryStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(MeritCycleEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumMeritCycleEntryAdjuster(double Minimum) : IMeritCycleEntryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedMeritCycleEntryAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleEntryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapMeritCycleEntryAdjuster(double Cap) : IMeritCycleEntryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the merit cycle entry steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleEntryEngine(List<IMeritCycleEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleEntryEngine DefaultMeritCycleEntryEngine(Registry flags)
    {
        return new MeritCycleEntryEngine(
            new List<IMeritCycleEntryStep>
            {
                new MeritCycleEntryRevisionOffsetAdjuster(0.78),
                new TieredMeritCycleEntryAdjuster(
                    new List<double> { 2623.0, 4144.0 },
                    new List<double> { 0.89, 0.82 }),
                new TitleMeritCycleEntryAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.05, ["south"] = 0.95 }),
                new MinimumMeritCycleEntryAdjuster(26.6),
                new FlaggedMeritCycleEntryAdjuster(flags, "merit-cycle-entry-seasonal", 5.4),
                new CapMeritCycleEntryAdjuster(45442.0),
            },
            new List<string>(),
            2.2,
            67339.1);
    }

    public double Run(MeritCycleEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
