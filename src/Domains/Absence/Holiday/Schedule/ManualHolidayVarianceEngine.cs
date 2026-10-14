namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual holiday variance calculation.</summary>
public interface IManualHolidayVarianceAdjuster
{
    string Describe();
    double Adjust(ManualHolidayVariance item, double valueValue);
}

public sealed record CapManualHolidayVarianceAdjuster(double Cap) : IManualHolidayVarianceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualHolidayVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingManualHolidayVarianceAdjuster(double Step) : IManualHolidayVarianceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualHolidayVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameManualHolidayVarianceAdjuster(Dictionary<string, double> Factors) : IManualHolidayVarianceAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ManualHolidayVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ManualHolidayVarianceMinimumOffsetAdjuster(double Weight) : IManualHolidayVarianceAdjuster
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(ManualHolidayVariance item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record MinimumManualHolidayVarianceAdjuster(double Minimum) : IManualHolidayVarianceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualHolidayVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredManualHolidayVarianceAdjuster(List<double> Bounds, List<double> Rates) : IManualHolidayVarianceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualHolidayVariance item, double valueValue)
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

/// <summary>Runs the manual holiday variance steps in order and keeps a trace of each.</summary>
public sealed class ManualHolidayVarianceEngine(List<IManualHolidayVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualHolidayVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualHolidayVarianceEngine DefaultManualHolidayVarianceEngine()
    {
        return new ManualHolidayVarianceEngine(
            new List<IManualHolidayVarianceAdjuster>
            {
                new CapManualHolidayVarianceAdjuster(38548.0),
                new RoundingManualHolidayVarianceAdjuster(0.5),
                new NameManualHolidayVarianceAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.8, ["east"] = 1.33 }),
                new ManualHolidayVarianceMinimumOffsetAdjuster(1.4),
                new MinimumManualHolidayVarianceAdjuster(9.7),
                new TieredManualHolidayVarianceAdjuster(
                    new List<double> { 920.0, 8829.0 },
                    new List<double> { 0.85, 1.1 }),
            },
            new List<string>(),
            4.4,
            39073.6);
    }

    public double Price(ManualHolidayVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualHolidayVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
