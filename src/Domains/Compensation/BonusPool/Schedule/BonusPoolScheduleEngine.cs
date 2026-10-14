namespace Acme.Workforce.Compensation.BonusPool.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool schedule calculation.</summary>
public interface IBonusPoolScheduleStep
{
    string Describe();
    double Adjust(BonusPoolSchedule item, double valueValue);
}

public sealed record FlaggedBonusPoolScheduleAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolScheduleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBonusPoolScheduleAdjuster(double Step) : IBonusPoolScheduleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BonusPoolSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleBonusPoolScheduleAdjuster(Dictionary<string, double> Factors) : IBonusPoolScheduleStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(BonusPoolSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the bonus pool schedule steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolScheduleEngine(List<IBonusPoolScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolScheduleEngine DefaultBonusPoolScheduleEngine(Registry flags)
    {
        return new BonusPoolScheduleEngine(
            new List<IBonusPoolScheduleStep>
            {
                new FlaggedBonusPoolScheduleAdjuster(flags, "bonus-pool-schedule-strict", 5.6),
                new RoundingBonusPoolScheduleAdjuster(5.0),
                new LocaleBonusPoolScheduleAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.23, ["south"] = 1.22, ["online"] = 0.74 }),
            },
            new List<string>(),
            2.2,
            21330.1);
    }

    public double Compute(BonusPoolSchedule item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
