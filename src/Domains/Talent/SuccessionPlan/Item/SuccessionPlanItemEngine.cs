namespace Acme.Workforce.Talent.SuccessionPlan.Item;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan item calculation.</summary>
public interface ISuccessionPlanItemAdjuster
{
    string Describe();
    double Adjust(SuccessionPlanItem item, double valueValue);
}

public sealed record RoundingSuccessionPlanItemAdjuster(double Step) : ISuccessionPlanItemAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanItem item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSuccessionPlanItemAdjuster(double Percent) : ISuccessionPlanItemAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanItem item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSuccessionPlanItemAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanItemAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanItem item, double valueValue)
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

public sealed record SuccessionPlanItemPriorityOffsetAdjuster(double Weight) : ISuccessionPlanItemAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(SuccessionPlanItem item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

/// <summary>Runs the succession plan item steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanItemEngine(List<ISuccessionPlanItemAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanItemAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanItemEngine DefaultSuccessionPlanItemEngine()
    {
        return new SuccessionPlanItemEngine(
            new List<ISuccessionPlanItemAdjuster>
            {
                new RoundingSuccessionPlanItemAdjuster(0.1),
                new PercentSuccessionPlanItemAdjuster(4.1),
                new TieredSuccessionPlanItemAdjuster(
                    new List<double> { 716.0, 2122.0 },
                    new List<double> { 0.87, 0.94 }),
                new SuccessionPlanItemPriorityOffsetAdjuster(0.67),
            },
            new List<string>(),
            2.1,
            36706.8);
    }

    public double Price(SuccessionPlanItem item)
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
