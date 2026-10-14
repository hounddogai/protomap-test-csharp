namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive timesheet draft calculation.</summary>
public interface IAdaptiveTimesheetDraftAdjuster
{
    string Describe();
    double Adjust(AdaptiveTimesheetDraft item, double valueValue);
}

public sealed record NameAdaptiveTimesheetDraftAdjuster(Dictionary<string, double> Factors) : IAdaptiveTimesheetDraftAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveTimesheetDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAdaptiveTimesheetDraftAdjuster(double Cap) : IAdaptiveTimesheetDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveTimesheetDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAdaptiveTimesheetDraftAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveTimesheetDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveTimesheetDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptiveTimesheetDraftThresholdOffsetAdjuster(double Weight) : IAdaptiveTimesheetDraftAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(AdaptiveTimesheetDraft item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record TieredAdaptiveTimesheetDraftAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveTimesheetDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveTimesheetDraft item, double valueValue)
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

/// <summary>Runs the adaptive timesheet draft steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTimesheetDraftEngine(List<IAdaptiveTimesheetDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTimesheetDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTimesheetDraftEngine DefaultAdaptiveTimesheetDraftEngine(Registry flags)
    {
        return new AdaptiveTimesheetDraftEngine(
            new List<IAdaptiveTimesheetDraftAdjuster>
            {
                new NameAdaptiveTimesheetDraftAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.06, ["central"] = 1.08 }),
                new CapAdaptiveTimesheetDraftAdjuster(8298.0),
                new FlaggedAdaptiveTimesheetDraftAdjuster(flags, "adaptive-timesheet-draft-pending", 4.4),
                new AdaptiveTimesheetDraftThresholdOffsetAdjuster(1.73),
                new TieredAdaptiveTimesheetDraftAdjuster(
                    new List<double> { 1690.0, 7999.0 },
                    new List<double> { 1.19, 0.9 }),
            },
            new List<string>(),
            1.8,
            58982.4);
    }

    public double Run(AdaptiveTimesheetDraft item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTimesheetDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
