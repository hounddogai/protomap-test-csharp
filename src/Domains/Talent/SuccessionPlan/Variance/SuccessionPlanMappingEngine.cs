namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan mapping calculation.</summary>
public interface ISuccessionPlanMappingStep
{
    string Describe();
    double Adjust(SuccessionPlanMapping item, double valueValue);
}

public sealed record CapSuccessionPlanMappingAdjuster(double Cap) : ISuccessionPlanMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentSuccessionPlanMappingAdjuster(double Percent) : ISuccessionPlanMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSuccessionPlanMappingAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanMapping item, double valueValue)
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

public sealed record RoundingSuccessionPlanMappingAdjuster(double Step) : ISuccessionPlanMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the succession plan mapping steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanMappingEngine(List<ISuccessionPlanMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanMappingEngine DefaultSuccessionPlanMappingEngine()
    {
        return new SuccessionPlanMappingEngine(
            new List<ISuccessionPlanMappingStep>
            {
                new CapSuccessionPlanMappingAdjuster(40739.0),
                new PercentSuccessionPlanMappingAdjuster(-16.8),
                new TieredSuccessionPlanMappingAdjuster(
                    new List<double> { 2850.0, 7961.0 },
                    new List<double> { 0.94, 0.95 }),
                new RoundingSuccessionPlanMappingAdjuster(0.1),
            },
            new List<string>(),
            4.1,
            9295.0);
    }

    public double Run(SuccessionPlanMapping item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
