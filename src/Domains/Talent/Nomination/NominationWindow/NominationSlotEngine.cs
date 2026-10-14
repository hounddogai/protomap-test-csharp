namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination slot calculation.</summary>
public interface INominationSlotStep
{
    string Describe();
    double Adjust(NominationSlot item, double valueValue);
}

public sealed record RoundingNominationSlotAdjuster(double Step) : INominationSlotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedNominationSlotAdjuster(Registry Flags, string Flag, double Percent) : INominationSlotStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamNominationSlotAdjuster(Dictionary<string, double> Factors) : INominationSlotStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(NominationSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record NominationSlotHoursOffsetAdjuster(double Weight) : INominationSlotStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(NominationSlot item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record CapNominationSlotAdjuster(double Cap) : INominationSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentNominationSlotAdjuster(double Percent) : INominationSlotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the nomination slot steps in order and keeps a trace of each.</summary>
public sealed class NominationSlotEngine(List<INominationSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationSlotEngine DefaultNominationSlotEngine(Registry flags)
    {
        return new NominationSlotEngine(
            new List<INominationSlotStep>
            {
                new RoundingNominationSlotAdjuster(0.25),
                new FlaggedNominationSlotAdjuster(flags, "nomination-slot-primary", 9.8),
                new TeamNominationSlotAdjuster(new Dictionary<string, double> { ["retail"] = 0.97 }),
                new NominationSlotHoursOffsetAdjuster(0.64),
                new CapNominationSlotAdjuster(37637.0),
                new PercentNominationSlotAdjuster(14.3),
            },
            new List<string>(),
            3.1,
            79954.1);
    }

    public double Evaluate(NominationSlot item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<NominationSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
