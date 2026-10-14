namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled statement of work segment calculation.</summary>
public interface IScheduledStatementOfWorkSegmentAdjuster
{
    string Describe();
    double Adjust(ScheduledStatementOfWorkSegment item, double valueValue);
}

public sealed record RoundingScheduledStatementOfWorkSegmentAdjuster(double Step) : IScheduledStatementOfWorkSegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledStatementOfWorkSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredScheduledStatementOfWorkSegmentAdjuster(List<double> Bounds, List<double> Rates) : IScheduledStatementOfWorkSegmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledStatementOfWorkSegment item, double valueValue)
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

public sealed record MinimumScheduledStatementOfWorkSegmentAdjuster(double Minimum) : IScheduledStatementOfWorkSegmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledStatementOfWorkSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapScheduledStatementOfWorkSegmentAdjuster(double Cap) : IScheduledStatementOfWorkSegmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledStatementOfWorkSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryScheduledStatementOfWorkSegmentAdjuster(Dictionary<string, double> Factors) : IScheduledStatementOfWorkSegmentAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ScheduledStatementOfWorkSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled statement of work segment steps in order and keeps a trace of each.</summary>
public sealed class ScheduledStatementOfWorkSegmentEngine(List<IScheduledStatementOfWorkSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledStatementOfWorkSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledStatementOfWorkSegmentEngine DefaultScheduledStatementOfWorkSegmentEngine()
    {
        return new ScheduledStatementOfWorkSegmentEngine(
            new List<IScheduledStatementOfWorkSegmentAdjuster>
            {
                new RoundingScheduledStatementOfWorkSegmentAdjuster(0.5),
                new TieredScheduledStatementOfWorkSegmentAdjuster(
                    new List<double> { 3029.0, 5853.0 },
                    new List<double> { 0.84, 1.03 }),
                new MinimumScheduledStatementOfWorkSegmentAdjuster(22.8),
                new CapScheduledStatementOfWorkSegmentAdjuster(30591.0),
                new CountryScheduledStatementOfWorkSegmentAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.95, ["central"] = 0.73, ["north"] = 0.91 }),
            },
            new List<string>(),
            4.6,
            87177.0);
    }

    public double Price(ScheduledStatementOfWorkSegment item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledStatementOfWorkSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
