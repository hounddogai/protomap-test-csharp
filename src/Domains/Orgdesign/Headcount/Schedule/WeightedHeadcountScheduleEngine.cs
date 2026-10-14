namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted headcount schedule calculation.</summary>
public interface IWeightedHeadcountScheduleModifier
{
    string Describe();
    double Adjust(WeightedHeadcountSchedule item, double valueValue);
}

public sealed record PercentWeightedHeadcountScheduleAdjuster(double Percent) : IWeightedHeadcountScheduleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedHeadcountSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamWeightedHeadcountScheduleAdjuster(Dictionary<string, double> Factors) : IWeightedHeadcountScheduleModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(WeightedHeadcountSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedWeightedHeadcountScheduleAdjuster(Registry Flags, string Flag, double Percent) : IWeightedHeadcountScheduleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedHeadcountSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedHeadcountScheduleAdjuster(List<double> Bounds, List<double> Rates) : IWeightedHeadcountScheduleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedHeadcountSchedule item, double valueValue)
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

public sealed record RoundingWeightedHeadcountScheduleAdjuster(double Step) : IWeightedHeadcountScheduleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedHeadcountSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted headcount schedule steps in order and keeps a trace of each.</summary>
public sealed class WeightedHeadcountScheduleEngine(List<IWeightedHeadcountScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedHeadcountScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedHeadcountScheduleEngine DefaultWeightedHeadcountScheduleEngine(Registry flags)
    {
        return new WeightedHeadcountScheduleEngine(
            new List<IWeightedHeadcountScheduleModifier>
            {
                new PercentWeightedHeadcountScheduleAdjuster(-9.1),
                new TeamWeightedHeadcountScheduleAdjuster(new Dictionary<string, double> { ["north"] = 1.38 }),
                new FlaggedWeightedHeadcountScheduleAdjuster(flags, "weighted-headcount-schedule-weighted", -3.1),
                new TieredWeightedHeadcountScheduleAdjuster(
                    new List<double> { 6961.0, 8853.0 },
                    new List<double> { 1.05, 0.83 }),
                new RoundingWeightedHeadcountScheduleAdjuster(5.0),
            },
            new List<string>(),
            1.1,
            70977.5);
    }

    public double Run(WeightedHeadcountSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedHeadcountSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
