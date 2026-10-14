namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling holiday mapping calculation.</summary>
public interface IRollingHolidayMappingFactor
{
    string Describe();
    double Adjust(RollingHolidayMapping item, double valueValue);
}

public sealed record TieredRollingHolidayMappingAdjuster(List<double> Bounds, List<double> Rates) : IRollingHolidayMappingFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingHolidayMapping item, double valueValue)
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

public sealed record RollingHolidayMappingPriorityOffsetAdjuster(double Weight) : IRollingHolidayMappingFactor
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(RollingHolidayMapping item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record TitleRollingHolidayMappingAdjuster(Dictionary<string, double> Factors) : IRollingHolidayMappingFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RollingHolidayMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the rolling holiday mapping steps in order and keeps a trace of each.</summary>
public sealed class RollingHolidayMappingEngine(List<IRollingHolidayMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingHolidayMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingHolidayMappingEngine DefaultRollingHolidayMappingEngine()
    {
        return new RollingHolidayMappingEngine(
            new List<IRollingHolidayMappingFactor>
            {
                new TieredRollingHolidayMappingAdjuster(
                    new List<double> { 7943.0, 9844.0 },
                    new List<double> { 0.89, 0.99 }),
                new RollingHolidayMappingPriorityOffsetAdjuster(1.35),
                new TitleRollingHolidayMappingAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.01, ["south"] = 1.37, ["retail"] = 0.97 }),
            },
            new List<string>(),
            2.4,
            53912.1);
    }

    public double Evaluate(RollingHolidayMapping item)
    {
        var current = item.ProjectedLoad();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RollingHolidayMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
