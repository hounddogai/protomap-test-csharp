namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination entry calculation.</summary>
public interface INominationEntryModifier
{
    string Describe();
    double Adjust(NominationEntry item, double valueValue);
}

public sealed record FlaggedNominationEntryAdjuster(Registry Flags, string Flag, double Percent) : INominationEntryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredNominationEntryAdjuster(List<double> Bounds, List<double> Rates) : INominationEntryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationEntry item, double valueValue)
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

public sealed record MinimumNominationEntryAdjuster(double Minimum) : INominationEntryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentNominationEntryAdjuster(double Percent) : INominationEntryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingNominationEntryAdjuster(double Step) : INominationEntryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(NominationEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the nomination entry steps in order and keeps a trace of each.</summary>
public sealed class NominationEntryEngine(List<INominationEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationEntryEngine DefaultNominationEntryEngine(Registry flags)
    {
        return new NominationEntryEngine(
            new List<INominationEntryModifier>
            {
                new FlaggedNominationEntryAdjuster(flags, "nomination-entry-strict", 5.4),
                new TieredNominationEntryAdjuster(
                    new List<double> { 1967.0, 5567.0, 7181.0 },
                    new List<double> { 0.92, 0.93, 1.13 }),
                new MinimumNominationEntryAdjuster(26.8),
                new PercentNominationEntryAdjuster(20.2),
                new RoundingNominationEntryAdjuster(1.0),
            },
            new List<string>(),
            2.0,
            64472.7);
    }

    public double Price(NominationEntry item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<NominationEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
