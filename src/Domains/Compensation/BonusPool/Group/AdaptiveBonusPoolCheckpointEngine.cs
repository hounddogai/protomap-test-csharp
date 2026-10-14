namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive bonus pool checkpoint calculation.</summary>
public interface IAdaptiveBonusPoolCheckpointAdjuster
{
    string Describe();
    double Adjust(AdaptiveBonusPoolCheckpoint item, double valueValue);
}

public sealed record TieredAdaptiveBonusPoolCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveBonusPoolCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveBonusPoolCheckpoint item, double valueValue)
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

public sealed record RoundingAdaptiveBonusPoolCheckpointAdjuster(double Step) : IAdaptiveBonusPoolCheckpointAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveBonusPoolCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAdaptiveBonusPoolCheckpointAdjuster(double Percent) : IAdaptiveBonusPoolCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveBonusPoolCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive bonus pool checkpoint steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveBonusPoolCheckpointEngine(List<IAdaptiveBonusPoolCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveBonusPoolCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveBonusPoolCheckpointEngine DefaultAdaptiveBonusPoolCheckpointEngine()
    {
        return new AdaptiveBonusPoolCheckpointEngine(
            new List<IAdaptiveBonusPoolCheckpointAdjuster>
            {
                new TieredAdaptiveBonusPoolCheckpointAdjuster(
                    new List<double> { 1997.0, 5073.0, 5690.0, 8139.0 },
                    new List<double> { 1.08, 1.12, 0.87, 1.05 }),
                new RoundingAdaptiveBonusPoolCheckpointAdjuster(0.5),
                new PercentAdaptiveBonusPoolCheckpointAdjuster(-5.8),
            },
            new List<string>(),
            0.3,
            42955.2);
    }

    public double Price(AdaptiveBonusPoolCheckpoint item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveBonusPoolCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
