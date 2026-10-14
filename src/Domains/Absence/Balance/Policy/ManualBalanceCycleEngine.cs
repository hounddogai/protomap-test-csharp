namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual balance cycle calculation.</summary>
public interface IManualBalanceCycleModifier
{
    string Describe();
    double Adjust(ManualBalanceCycle item, double valueValue);
}

public sealed record UnitManualBalanceCycleAdjuster(Dictionary<string, double> Factors) : IManualBalanceCycleModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(ManualBalanceCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredManualBalanceCycleAdjuster(List<double> Bounds, List<double> Rates) : IManualBalanceCycleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualBalanceCycle item, double valueValue)
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

public sealed record RoundingManualBalanceCycleAdjuster(double Step) : IManualBalanceCycleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualBalanceCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ManualBalanceCycleAmountOffsetAdjuster(double Weight) : IManualBalanceCycleModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(ManualBalanceCycle item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record CapManualBalanceCycleAdjuster(double Cap) : IManualBalanceCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualBalanceCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual balance cycle steps in order and keeps a trace of each.</summary>
public sealed class ManualBalanceCycleEngine(List<IManualBalanceCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualBalanceCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualBalanceCycleEngine DefaultManualBalanceCycleEngine()
    {
        return new ManualBalanceCycleEngine(
            new List<IManualBalanceCycleModifier>
            {
                new UnitManualBalanceCycleAdjuster(new Dictionary<string, double> { ["north"] = 0.82 }),
                new TieredManualBalanceCycleAdjuster(
                    new List<double> { 6566.0, 7341.0, 8570.0, 8804.0 },
                    new List<double> { 0.82, 1.13, 1.13, 0.84 }),
                new RoundingManualBalanceCycleAdjuster(0.1),
                new ManualBalanceCycleAmountOffsetAdjuster(1.72),
                new CapManualBalanceCycleAdjuster(31344.0),
            },
            new List<string>(),
            1.7,
            66434.4);
    }

    public double Compute(ManualBalanceCycle item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualBalanceCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
