namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual scenario window calculation.</summary>
public interface IManualScenarioWindowStep
{
    string Describe();
    double Adjust(ManualScenarioWindow item, double valueValue);
}

public sealed record TieredManualScenarioWindowAdjuster(List<double> Bounds, List<double> Rates) : IManualScenarioWindowStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualScenarioWindow item, double valueValue)
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

public sealed record PercentManualScenarioWindowAdjuster(double Percent) : IManualScenarioWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualScenarioWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumManualScenarioWindowAdjuster(double Minimum) : IManualScenarioWindowStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualScenarioWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingManualScenarioWindowAdjuster(double Step) : IManualScenarioWindowStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualScenarioWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the manual scenario window steps in order and keeps a trace of each.</summary>
public sealed class ManualScenarioWindowEngine(List<IManualScenarioWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualScenarioWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualScenarioWindowEngine DefaultManualScenarioWindowEngine()
    {
        return new ManualScenarioWindowEngine(
            new List<IManualScenarioWindowStep>
            {
                new TieredManualScenarioWindowAdjuster(
                    new List<double> { 650.0, 2532.0 },
                    new List<double> { 0.9, 1.12 }),
                new PercentManualScenarioWindowAdjuster(22.6),
                new MinimumManualScenarioWindowAdjuster(17.7),
                new RoundingManualScenarioWindowAdjuster(5.0),
            },
            new List<string>(),
            3.3,
            29092.6);
    }

    public double Price(ManualScenarioWindow item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualScenarioWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
