namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted scenario mapping calculation.</summary>
public interface IWeightedScenarioMappingStep
{
    string Describe();
    double Adjust(WeightedScenarioMapping item, double valueValue);
}

public sealed record RoundingWeightedScenarioMappingAdjuster(double Step) : IWeightedScenarioMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedScenarioMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WeightedScenarioMappingRatioOffsetAdjuster(double Weight) : IWeightedScenarioMappingStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(WeightedScenarioMapping item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record DepartmentWeightedScenarioMappingAdjuster(Dictionary<string, double> Factors) : IWeightedScenarioMappingStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(WeightedScenarioMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumWeightedScenarioMappingAdjuster(double Minimum) : IWeightedScenarioMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedScenarioMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted scenario mapping steps in order and keeps a trace of each.</summary>
public sealed class WeightedScenarioMappingEngine(List<IWeightedScenarioMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedScenarioMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedScenarioMappingEngine DefaultWeightedScenarioMappingEngine()
    {
        return new WeightedScenarioMappingEngine(
            new List<IWeightedScenarioMappingStep>
            {
                new RoundingWeightedScenarioMappingAdjuster(0.1),
                new WeightedScenarioMappingRatioOffsetAdjuster(0.96),
                new DepartmentWeightedScenarioMappingAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.89, ["north"] = 0.78, ["retail"] = 1.11 }),
                new MinimumWeightedScenarioMappingAdjuster(0.6),
            },
            new List<string>(),
            0.8,
            98378.1);
    }

    public double Evaluate(WeightedScenarioMapping item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedScenarioMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
