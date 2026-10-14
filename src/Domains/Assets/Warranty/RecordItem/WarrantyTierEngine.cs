namespace Acme.Workforce.Assets.Warranty.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty tier calculation.</summary>
public interface IWarrantyTierModifier
{
    string Describe();
    double Adjust(WarrantyTier item, double valueValue);
}

public sealed record RoundingWarrantyTierAdjuster(double Step) : IWarrantyTierModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WarrantyTierCostOffsetAdjuster(double Weight) : IWarrantyTierModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(WarrantyTier item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record SourceWarrantyTierAdjuster(Dictionary<string, double> Factors) : IWarrantyTierModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(WarrantyTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the warranty tier steps in order and keeps a trace of each.</summary>
public sealed class WarrantyTierEngine(List<IWarrantyTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyTierEngine DefaultWarrantyTierEngine()
    {
        return new WarrantyTierEngine(
            new List<IWarrantyTierModifier>
            {
                new RoundingWarrantyTierAdjuster(5.0),
                new WarrantyTierCostOffsetAdjuster(0.46),
                new SourceWarrantyTierAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.86, ["online"] = 1.19, ["south"] = 1.03 }),
            },
            new List<string>(),
            4.0,
            82771.1);
    }

    public double Compute(WarrantyTier item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
