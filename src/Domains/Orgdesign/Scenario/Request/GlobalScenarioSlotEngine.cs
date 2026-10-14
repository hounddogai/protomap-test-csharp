namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global scenario slot calculation.</summary>
public interface IGlobalScenarioSlotStep
{
    string Describe();
    double Adjust(GlobalScenarioSlot item, double valueValue);
}

public sealed record TieredGlobalScenarioSlotAdjuster(List<double> Bounds, List<double> Rates) : IGlobalScenarioSlotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalScenarioSlot item, double valueValue)
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

public sealed record RoundingGlobalScenarioSlotAdjuster(double Step) : IGlobalScenarioSlotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalScenarioSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentGlobalScenarioSlotAdjuster(double Percent) : IGlobalScenarioSlotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalScenarioSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGlobalScenarioSlotAdjuster(double Cap) : IGlobalScenarioSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalScenarioSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record GlobalScenarioSlotWindowDaysOffsetAdjuster(double Weight) : IGlobalScenarioSlotStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(GlobalScenarioSlot item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

/// <summary>Runs the global scenario slot steps in order and keeps a trace of each.</summary>
public sealed class GlobalScenarioSlotEngine(List<IGlobalScenarioSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalScenarioSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalScenarioSlotEngine DefaultGlobalScenarioSlotEngine()
    {
        return new GlobalScenarioSlotEngine(
            new List<IGlobalScenarioSlotStep>
            {
                new TieredGlobalScenarioSlotAdjuster(
                    new List<double> { 1201.0, 2660.0, 3683.0, 7108.0 },
                    new List<double> { 1.18, 0.94, 1.1, 1.12 }),
                new RoundingGlobalScenarioSlotAdjuster(0.05),
                new PercentGlobalScenarioSlotAdjuster(12.6),
                new CapGlobalScenarioSlotAdjuster(28835.0),
                new GlobalScenarioSlotWindowDaysOffsetAdjuster(1.11),
            },
            new List<string>(),
            4.8,
            58631.9);
    }

    public double Compute(GlobalScenarioSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalScenarioSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
