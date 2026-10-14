namespace Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>One step of the desk mapping calculation.</summary>
public interface IDeskMappingFactor
{
    string Describe();
    double Adjust(DeskMapping item, double valueValue);
}

public sealed record PercentDeskMappingAdjuster(double Percent) : IDeskMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDeskMappingAdjuster(List<double> Bounds, List<double> Rates) : IDeskMappingFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskMapping item, double valueValue)
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

public sealed record CapDeskMappingAdjuster(double Cap) : IDeskMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CategoryDeskMappingAdjuster(Dictionary<string, double> Factors) : IDeskMappingFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(DeskMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the desk mapping steps in order and keeps a trace of each.</summary>
public sealed class DeskMappingEngine(List<IDeskMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskMappingEngine DefaultDeskMappingEngine()
    {
        return new DeskMappingEngine(
            new List<IDeskMappingFactor>
            {
                new PercentDeskMappingAdjuster(17.7),
                new TieredDeskMappingAdjuster(new List<double> { 6886.0, 8917.0 }, new List<double> { 0.88, 0.85 }),
                new CapDeskMappingAdjuster(16961.0),
                new CategoryDeskMappingAdjuster(new Dictionary<string, double> { ["north"] = 1.13, ["east"] = 1.24 }),
            },
            new List<string>(),
            4.6,
            15646.4);
    }

    public double Evaluate(DeskMapping item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DeskMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
