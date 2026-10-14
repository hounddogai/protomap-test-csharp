namespace Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>One step of the regional holiday cycle calculation.</summary>
public interface IRegionalHolidayCycleAdjuster
{
    string Describe();
    double Adjust(RegionalHolidayCycle item, double valueValue);
}

public sealed record MinimumRegionalHolidayCycleAdjuster(double Minimum) : IRegionalHolidayCycleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalHolidayCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RegionalHolidayCycleBudgetOffsetAdjuster(double Weight) : IRegionalHolidayCycleAdjuster
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(RegionalHolidayCycle item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record TieredRegionalHolidayCycleAdjuster(List<double> Bounds, List<double> Rates) : IRegionalHolidayCycleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalHolidayCycle item, double valueValue)
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

public sealed record DepartmentRegionalHolidayCycleAdjuster(Dictionary<string, double> Factors) : IRegionalHolidayCycleAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(RegionalHolidayCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRegionalHolidayCycleAdjuster(double Cap) : IRegionalHolidayCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalHolidayCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the regional holiday cycle steps in order and keeps a trace of each.</summary>
public sealed class RegionalHolidayCycleEngine(List<IRegionalHolidayCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalHolidayCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalHolidayCycleEngine DefaultRegionalHolidayCycleEngine()
    {
        return new RegionalHolidayCycleEngine(
            new List<IRegionalHolidayCycleAdjuster>
            {
                new MinimumRegionalHolidayCycleAdjuster(29.1),
                new RegionalHolidayCycleBudgetOffsetAdjuster(1.41),
                new TieredRegionalHolidayCycleAdjuster(
                    new List<double> { 1064.0, 3839.0, 5369.0, 8932.0 },
                    new List<double> { 1.01, 1.02, 0.96, 1.18 }),
                new DepartmentRegionalHolidayCycleAdjuster(new Dictionary<string, double> { ["south"] = 0.81 }),
                new CapRegionalHolidayCycleAdjuster(16890.0),
            },
            new List<string>(),
            4.4,
            48306.2);
    }

    public double Price(RegionalHolidayCycle item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalHolidayCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
