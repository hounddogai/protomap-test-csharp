namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted bonus pool notice calculation.</summary>
public interface IWeightedBonusPoolNoticeModifier
{
    string Describe();
    double Adjust(WeightedBonusPoolNotice item, double valueValue);
}

public sealed record NameWeightedBonusPoolNoticeAdjuster(Dictionary<string, double> Factors) : IWeightedBonusPoolNoticeModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(WeightedBonusPoolNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapWeightedBonusPoolNoticeAdjuster(double Cap) : IWeightedBonusPoolNoticeModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedBonusPoolNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredWeightedBonusPoolNoticeAdjuster(List<double> Bounds, List<double> Rates) : IWeightedBonusPoolNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedBonusPoolNotice item, double valueValue)
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

/// <summary>Runs the weighted bonus pool notice steps in order and keeps a trace of each.</summary>
public sealed class WeightedBonusPoolNoticeEngine(List<IWeightedBonusPoolNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedBonusPoolNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedBonusPoolNoticeEngine DefaultWeightedBonusPoolNoticeEngine()
    {
        return new WeightedBonusPoolNoticeEngine(
            new List<IWeightedBonusPoolNoticeModifier>
            {
                new NameWeightedBonusPoolNoticeAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.21, ["north"] = 1.02, ["retail"] = 1.11 }),
                new CapWeightedBonusPoolNoticeAdjuster(7995.0),
                new TieredWeightedBonusPoolNoticeAdjuster(
                    new List<double> { 2491.0, 7731.0 },
                    new List<double> { 0.91, 0.94 }),
            },
            new List<string>(),
            2.9,
            94817.1);
    }

    public double Evaluate(WeightedBonusPoolNotice item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedBonusPoolNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
