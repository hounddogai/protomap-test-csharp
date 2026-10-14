namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination schedule calculation.</summary>
public interface INominationScheduleModifier
{
    string Describe();
    double Adjust(NominationSchedule item, double valueValue);
}

public sealed record NameNominationScheduleAdjuster(Dictionary<string, double> Factors) : INominationScheduleModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(NominationSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredNominationScheduleAdjuster(List<double> Bounds, List<double> Rates) : INominationScheduleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationSchedule item, double valueValue)
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

public sealed record PercentNominationScheduleAdjuster(double Percent) : INominationScheduleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedNominationScheduleAdjuster(Registry Flags, string Flag, double Percent) : INominationScheduleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumNominationScheduleAdjuster(double Minimum) : INominationScheduleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the nomination schedule steps in order and keeps a trace of each.</summary>
public sealed class NominationScheduleEngine(List<INominationScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationScheduleEngine DefaultNominationScheduleEngine(Registry flags)
    {
        return new NominationScheduleEngine(
            new List<INominationScheduleModifier>
            {
                new NameNominationScheduleAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.27, ["north"] = 1.34 }),
                new TieredNominationScheduleAdjuster(
                    new List<double> { 266.0, 9825.0 },
                    new List<double> { 0.9, 1.12 }),
                new PercentNominationScheduleAdjuster(20.0),
                new FlaggedNominationScheduleAdjuster(flags, "nomination-schedule-tiered", -2.2),
                new MinimumNominationScheduleAdjuster(38.7),
            },
            new List<string>(),
            1.5,
            30587.9);
    }

    public double Evaluate(NominationSchedule item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<NominationSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
