namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday request calculation.</summary>
public interface IHolidayRequestModifier
{
    string Describe();
    double Adjust(HolidayRequest item, double valueValue);
}

public sealed record MinimumHolidayRequestAdjuster(double Minimum) : IHolidayRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredHolidayRequestAdjuster(List<double> Bounds, List<double> Rates) : IHolidayRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayRequest item, double valueValue)
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

public sealed record RoundingHolidayRequestAdjuster(double Step) : IHolidayRequestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the holiday request steps in order and keeps a trace of each.</summary>
public sealed class HolidayRequestEngine(List<IHolidayRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayRequestEngine DefaultHolidayRequestEngine()
    {
        return new HolidayRequestEngine(
            new List<IHolidayRequestModifier>
            {
                new MinimumHolidayRequestAdjuster(38.9),
                new TieredHolidayRequestAdjuster(
                    new List<double> { 1851.0, 2368.0, 7420.0, 9131.0 },
                    new List<double> { 1.07, 1.03, 0.84, 1.02 }),
                new RoundingHolidayRequestAdjuster(5.0),
            },
            new List<string>(),
            3.5,
            77935.1);
    }

    public double Run(HolidayRequest item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
