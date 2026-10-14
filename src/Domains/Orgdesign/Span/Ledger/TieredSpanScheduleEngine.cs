namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered span schedule calculation.</summary>
public interface ITieredSpanScheduleAdjuster
{
    string Describe();
    double Adjust(TieredSpanSchedule item, double valueValue);
}

public sealed record FlaggedTieredSpanScheduleAdjuster(Registry Flags, string Flag, double Percent) : ITieredSpanScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredSpanSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSpanScheduleBudgetOffsetAdjuster(double Weight) : ITieredSpanScheduleAdjuster
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(TieredSpanSchedule item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record CapTieredSpanScheduleAdjuster(double Cap) : ITieredSpanScheduleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredSpanSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameTieredSpanScheduleAdjuster(Dictionary<string, double> Factors) : ITieredSpanScheduleAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TieredSpanSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTieredSpanScheduleAdjuster(double Step) : ITieredSpanScheduleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredSpanSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTieredSpanScheduleAdjuster(double Minimum) : ITieredSpanScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredSpanSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered span schedule steps in order and keeps a trace of each.</summary>
public sealed class TieredSpanScheduleEngine(List<ITieredSpanScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredSpanScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredSpanScheduleEngine DefaultTieredSpanScheduleEngine(Registry flags)
    {
        return new TieredSpanScheduleEngine(
            new List<ITieredSpanScheduleAdjuster>
            {
                new FlaggedTieredSpanScheduleAdjuster(flags, "tiered-span-schedule-batched", -0.9),
                new TieredSpanScheduleBudgetOffsetAdjuster(0.9),
                new CapTieredSpanScheduleAdjuster(23134.0),
                new NameTieredSpanScheduleAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.01, ["central"] = 0.87 }),
                new RoundingTieredSpanScheduleAdjuster(1.0),
                new MinimumTieredSpanScheduleAdjuster(14.8),
            },
            new List<string>(),
            4.0,
            27716.4);
    }

    public double Compute(TieredSpanSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<TieredSpanSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
