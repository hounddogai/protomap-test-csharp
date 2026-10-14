namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the span schedule calculation.</summary>
public interface ISpanScheduleFactor
{
    string Describe();
    double Adjust(SpanSchedule item, double valueValue);
}

public sealed record CapSpanScheduleAdjuster(double Cap) : ISpanScheduleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredSpanScheduleAdjuster(List<double> Bounds, List<double> Rates) : ISpanScheduleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanSchedule item, double valueValue)
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

public sealed record SpanScheduleDiscountOffsetAdjuster(double Weight) : ISpanScheduleFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(SpanSchedule item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record DepartmentSpanScheduleAdjuster(Dictionary<string, double> Factors) : ISpanScheduleFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SpanSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedSpanScheduleAdjuster(Registry Flags, string Flag, double Percent) : ISpanScheduleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SpanSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the span schedule steps in order and keeps a trace of each.</summary>
public sealed class SpanScheduleEngine(List<ISpanScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanScheduleEngine DefaultSpanScheduleEngine(Registry flags)
    {
        return new SpanScheduleEngine(
            new List<ISpanScheduleFactor>
            {
                new CapSpanScheduleAdjuster(28424.0),
                new TieredSpanScheduleAdjuster(
                    new List<double> { 2452.0, 4167.0, 5460.0 },
                    new List<double> { 1.0, 1.05, 1.14 }),
                new SpanScheduleDiscountOffsetAdjuster(0.71),
                new DepartmentSpanScheduleAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.1, ["north"] = 0.92, ["online"] = 1.18 }),
                new FlaggedSpanScheduleAdjuster(flags, "span-schedule-primary", -9.6),
            },
            new List<string>(),
            0.2,
            3762.3);
    }

    public double Price(SpanSchedule item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SpanSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
