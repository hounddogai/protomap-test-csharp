namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled equity grant variance calculation.</summary>
public interface IScheduledEquityGrantVarianceModifier
{
    string Describe();
    double Adjust(ScheduledEquityGrantVariance item, double valueValue);
}

public sealed record TieredScheduledEquityGrantVarianceAdjuster(List<double> Bounds, List<double> Rates) : IScheduledEquityGrantVarianceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledEquityGrantVariance item, double valueValue)
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

public sealed record RoundingScheduledEquityGrantVarianceAdjuster(double Step) : IScheduledEquityGrantVarianceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledEquityGrantVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ScheduledEquityGrantVarianceBatchSizeOffsetAdjuster(double Weight) : IScheduledEquityGrantVarianceModifier
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(ScheduledEquityGrantVariance item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record CapScheduledEquityGrantVarianceAdjuster(double Cap) : IScheduledEquityGrantVarianceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledEquityGrantVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScheduledEquityGrantVarianceAdjuster(double Minimum) : IScheduledEquityGrantVarianceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledEquityGrantVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentScheduledEquityGrantVarianceAdjuster(double Percent) : IScheduledEquityGrantVarianceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledEquityGrantVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled equity grant variance steps in order and keeps a trace of each.</summary>
public sealed class ScheduledEquityGrantVarianceEngine(List<IScheduledEquityGrantVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledEquityGrantVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledEquityGrantVarianceEngine DefaultScheduledEquityGrantVarianceEngine()
    {
        return new ScheduledEquityGrantVarianceEngine(
            new List<IScheduledEquityGrantVarianceModifier>
            {
                new TieredScheduledEquityGrantVarianceAdjuster(
                    new List<double> { 362.0, 901.0, 2417.0 },
                    new List<double> { 1.19, 0.83, 1.13 }),
                new RoundingScheduledEquityGrantVarianceAdjuster(0.25),
                new ScheduledEquityGrantVarianceBatchSizeOffsetAdjuster(0.87),
                new CapScheduledEquityGrantVarianceAdjuster(1789.0),
                new MinimumScheduledEquityGrantVarianceAdjuster(1.2),
                new PercentScheduledEquityGrantVarianceAdjuster(-13.4),
            },
            new List<string>(),
            1.5,
            4012.6);
    }

    public double Price(ScheduledEquityGrantVariance item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledEquityGrantVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
