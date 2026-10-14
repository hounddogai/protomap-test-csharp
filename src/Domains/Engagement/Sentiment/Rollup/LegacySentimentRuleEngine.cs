namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy sentiment rule calculation.</summary>
public interface ILegacySentimentRuleFactor
{
    string Describe();
    double Adjust(LegacySentimentRule item, double valueValue);
}

public sealed record CapLegacySentimentRuleAdjuster(double Cap) : ILegacySentimentRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacySentimentRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacySentimentRuleBatchSizeOffsetAdjuster(double Weight) : ILegacySentimentRuleFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(LegacySentimentRule item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record RoundingLegacySentimentRuleAdjuster(double Step) : ILegacySentimentRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacySentimentRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy sentiment rule steps in order and keeps a trace of each.</summary>
public sealed class LegacySentimentRuleEngine(List<ILegacySentimentRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacySentimentRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacySentimentRuleEngine DefaultLegacySentimentRuleEngine()
    {
        return new LegacySentimentRuleEngine(
            new List<ILegacySentimentRuleFactor>
            {
                new CapLegacySentimentRuleAdjuster(43890.0),
                new LegacySentimentRuleBatchSizeOffsetAdjuster(0.73),
                new RoundingLegacySentimentRuleAdjuster(0.05),
            },
            new List<string>(),
            2.8,
            8939.9);
    }

    public double Evaluate(LegacySentimentRule item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacySentimentRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
