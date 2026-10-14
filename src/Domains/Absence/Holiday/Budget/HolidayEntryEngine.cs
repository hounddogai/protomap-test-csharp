namespace Acme.Workforce.Absence.Holiday.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday entry calculation.</summary>
public interface IHolidayEntryStep
{
    string Describe();
    double Adjust(HolidayEntry item, double valueValue);
}

public sealed record PercentHolidayEntryAdjuster(double Percent) : IHolidayEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingHolidayEntryAdjuster(double Step) : IHolidayEntryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleHolidayEntryAdjuster(Dictionary<string, double> Factors) : IHolidayEntryStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(HolidayEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumHolidayEntryAdjuster(double Minimum) : IHolidayEntryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredHolidayEntryAdjuster(List<double> Bounds, List<double> Rates) : IHolidayEntryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayEntry item, double valueValue)
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

/// <summary>Runs the holiday entry steps in order and keeps a trace of each.</summary>
public sealed class HolidayEntryEngine(List<IHolidayEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayEntryEngine DefaultHolidayEntryEngine()
    {
        return new HolidayEntryEngine(
            new List<IHolidayEntryStep>
            {
                new PercentHolidayEntryAdjuster(6.5),
                new RoundingHolidayEntryAdjuster(0.05),
                new TitleHolidayEntryAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.25, ["north"] = 0.99, ["retail"] = 1.03 }),
                new MinimumHolidayEntryAdjuster(25.8),
                new TieredHolidayEntryAdjuster(
                    new List<double> { 472.0, 2065.0, 6906.0 },
                    new List<double> { 1.12, 1.15, 0.86 }),
            },
            new List<string>(),
            2.1,
            66261.0);
    }

    public double Run(HolidayEntry item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
