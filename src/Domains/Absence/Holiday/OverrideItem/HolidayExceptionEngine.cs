namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday exception calculation.</summary>
public interface IHolidayExceptionAdjuster
{
    string Describe();
    double Adjust(HolidayException item, double valueValue);
}

public sealed record HolidayExceptionRatingOffsetAdjuster(double Weight) : IHolidayExceptionAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(HolidayException item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record RoundingHolidayExceptionAdjuster(double Step) : IHolidayExceptionAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredHolidayExceptionAdjuster(List<double> Bounds, List<double> Rates) : IHolidayExceptionAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayException item, double valueValue)
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

/// <summary>Runs the holiday exception steps in order and keeps a trace of each.</summary>
public sealed class HolidayExceptionEngine(List<IHolidayExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayExceptionEngine DefaultHolidayExceptionEngine()
    {
        return new HolidayExceptionEngine(
            new List<IHolidayExceptionAdjuster>
            {
                new HolidayExceptionRatingOffsetAdjuster(0.58),
                new RoundingHolidayExceptionAdjuster(0.25),
                new TieredHolidayExceptionAdjuster(
                    new List<double> { 3971.0, 8703.0, 9382.0 },
                    new List<double> { 0.9, 0.81, 0.83 }),
            },
            new List<string>(),
            0.1,
            14700.1);
    }

    public double Run(HolidayException item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
