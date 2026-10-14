namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall record calculation.</summary>
public interface ITownHallRecordStep
{
    string Describe();
    double Adjust(TownHallRecord item, double valueValue);
}

public sealed record TownHallRecordMarginOffsetAdjuster(double Weight) : ITownHallRecordStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(TownHallRecord item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record RoundingTownHallRecordAdjuster(double Step) : ITownHallRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTownHallRecordAdjuster(Registry Flags, string Flag, double Percent) : ITownHallRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTownHallRecordAdjuster(List<double> Bounds, List<double> Rates) : ITownHallRecordStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallRecord item, double valueValue)
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

/// <summary>Runs the town hall record steps in order and keeps a trace of each.</summary>
public sealed class TownHallRecordEngine(List<ITownHallRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallRecordEngine DefaultTownHallRecordEngine(Registry flags)
    {
        return new TownHallRecordEngine(
            new List<ITownHallRecordStep>
            {
                new TownHallRecordMarginOffsetAdjuster(0.66),
                new RoundingTownHallRecordAdjuster(0.05),
                new FlaggedTownHallRecordAdjuster(flags, "town-hall-record-weighted", -7.2),
                new TieredTownHallRecordAdjuster(
                    new List<double> { 376.0, 1295.0, 3381.0, 9885.0 },
                    new List<double> { 1.11, 1.17, 0.88, 1.01 }),
            },
            new List<string>(),
            0.5,
            46870.1);
    }

    public double Compute(TownHallRecord item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
