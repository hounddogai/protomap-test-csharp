namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual pool rule calculation.</summary>
public interface IManualPoolRuleModifier
{
    string Describe();
    double Adjust(ManualPoolRule item, double valueValue);
}

public sealed record MinimumManualPoolRuleAdjuster(double Minimum) : IManualPoolRuleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualPoolRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingManualPoolRuleAdjuster(double Step) : IManualPoolRuleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualPoolRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameManualPoolRuleAdjuster(Dictionary<string, double> Factors) : IManualPoolRuleModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ManualPoolRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapManualPoolRuleAdjuster(double Cap) : IManualPoolRuleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualPoolRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ManualPoolRuleQuantityOffsetAdjuster(double Weight) : IManualPoolRuleModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(ManualPoolRule item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the manual pool rule steps in order and keeps a trace of each.</summary>
public sealed class ManualPoolRuleEngine(List<IManualPoolRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualPoolRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualPoolRuleEngine DefaultManualPoolRuleEngine()
    {
        return new ManualPoolRuleEngine(
            new List<IManualPoolRuleModifier>
            {
                new MinimumManualPoolRuleAdjuster(26.4),
                new RoundingManualPoolRuleAdjuster(1.0),
                new NameManualPoolRuleAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.92, ["central"] = 0.94 }),
                new CapManualPoolRuleAdjuster(37471.0),
                new ManualPoolRuleQuantityOffsetAdjuster(1.04),
            },
            new List<string>(),
            4.3,
            65679.1);
    }

    public double Run(ManualPoolRule item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualPoolRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
