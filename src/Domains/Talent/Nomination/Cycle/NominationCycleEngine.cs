namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination cycle calculation.</summary>
public interface INominationCycleModifier
{
    string Describe();
    double Adjust(NominationCycle item, double valueValue);
}

public sealed record CountryNominationCycleAdjuster(Dictionary<string, double> Factors) : INominationCycleModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(NominationCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapNominationCycleAdjuster(double Cap) : INominationCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingNominationCycleAdjuster(double Step) : INominationCycleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedNominationCycleAdjuster(Registry Flags, string Flag, double Percent) : INominationCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the nomination cycle steps in order and keeps a trace of each.</summary>
public sealed class NominationCycleEngine(List<INominationCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationCycleEngine DefaultNominationCycleEngine(Registry flags)
    {
        return new NominationCycleEngine(
            new List<INominationCycleModifier>
            {
                new CountryNominationCycleAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.91, ["online"] = 1.17 }),
                new CapNominationCycleAdjuster(8373.0),
                new RoundingNominationCycleAdjuster(0.05),
                new FlaggedNominationCycleAdjuster(flags, "nomination-cycle-pending", -6.4),
            },
            new List<string>(),
            1.5,
            95943.1);
    }

    public double Price(NominationCycle item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<NominationCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
