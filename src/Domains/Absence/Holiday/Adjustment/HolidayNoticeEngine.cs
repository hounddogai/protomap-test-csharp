namespace Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>One step of the holiday notice calculation.</summary>
public interface IHolidayNoticeStep
{
    string Describe();
    double Adjust(HolidayNotice item, double valueValue);
}

public sealed record RegionHolidayNoticeAdjuster(Dictionary<string, double> Factors) : IHolidayNoticeStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(HolidayNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumHolidayNoticeAdjuster(double Minimum) : IHolidayNoticeStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredHolidayNoticeAdjuster(List<double> Bounds, List<double> Rates) : IHolidayNoticeStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayNotice item, double valueValue)
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

/// <summary>Runs the holiday notice steps in order and keeps a trace of each.</summary>
public sealed class HolidayNoticeEngine(List<IHolidayNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayNoticeEngine DefaultHolidayNoticeEngine()
    {
        return new HolidayNoticeEngine(
            new List<IHolidayNoticeStep>
            {
                new RegionHolidayNoticeAdjuster(new Dictionary<string, double> { ["south"] = 1.25 }),
                new MinimumHolidayNoticeAdjuster(28.9),
                new TieredHolidayNoticeAdjuster(
                    new List<double> { 3388.0, 4480.0, 6857.0 },
                    new List<double> { 1.01, 0.82, 0.82 }),
            },
            new List<string>(),
            3.6,
            1549.3);
    }

    public double Compute(HolidayNotice item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
