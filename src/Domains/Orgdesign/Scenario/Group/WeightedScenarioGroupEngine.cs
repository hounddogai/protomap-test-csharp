namespace Acme.Workforce.Orgdesign.Scenario.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted scenario group calculation.</summary>
public interface IWeightedScenarioGroupModifier
{
    string Describe();
    double Adjust(WeightedScenarioGroup item, double valueValue);
}

public sealed record PercentWeightedScenarioGroupAdjuster(double Percent) : IWeightedScenarioGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedScenarioGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedScenarioGroupAdjuster(double Step) : IWeightedScenarioGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedScenarioGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumWeightedScenarioGroupAdjuster(double Minimum) : IWeightedScenarioGroupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedScenarioGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted scenario group steps in order and keeps a trace of each.</summary>
public sealed class WeightedScenarioGroupEngine(List<IWeightedScenarioGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedScenarioGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedScenarioGroupEngine DefaultWeightedScenarioGroupEngine()
    {
        return new WeightedScenarioGroupEngine(
            new List<IWeightedScenarioGroupModifier>
            {
                new PercentWeightedScenarioGroupAdjuster(20.7),
                new RoundingWeightedScenarioGroupAdjuster(0.05),
                new MinimumWeightedScenarioGroupAdjuster(40.4),
            },
            new List<string>(),
            2.3,
            92914.1);
    }

    public double Price(WeightedScenarioGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedScenarioGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
