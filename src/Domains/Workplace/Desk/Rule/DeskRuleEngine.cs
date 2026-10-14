namespace Acme.Workforce.Workplace.Desk.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk rule calculation.</summary>
public interface IDeskRuleAdjuster
{
    string Describe();
    double Adjust(DeskRule item, double valueValue);
}

public sealed record SourceDeskRuleAdjuster(Dictionary<string, double> Factors) : IDeskRuleAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(DeskRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDeskRuleAdjuster(List<double> Bounds, List<double> Rates) : IDeskRuleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskRule item, double valueValue)
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

public sealed record CapDeskRuleAdjuster(double Cap) : IDeskRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the desk rule steps in order and keeps a trace of each.</summary>
public sealed class DeskRuleEngine(List<IDeskRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskRuleEngine DefaultDeskRuleEngine()
    {
        return new DeskRuleEngine(
            new List<IDeskRuleAdjuster>
            {
                new SourceDeskRuleAdjuster(new Dictionary<string, double> { ["central"] = 1.36 }),
                new TieredDeskRuleAdjuster(
                    new List<double> { 3428.0, 6189.0, 7587.0, 8144.0 },
                    new List<double> { 1.2, 0.93, 1.11, 0.88 }),
                new CapDeskRuleAdjuster(15403.0),
            },
            new List<string>(),
            0.0,
            54917.5);
    }

    public double Evaluate(DeskRule item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DeskRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
