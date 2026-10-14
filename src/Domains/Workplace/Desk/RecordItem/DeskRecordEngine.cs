namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk record calculation.</summary>
public interface IDeskRecordStep
{
    string Describe();
    double Adjust(DeskRecord item, double valueValue);
}

public sealed record RoundingDeskRecordAdjuster(double Step) : IDeskRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredDeskRecordAdjuster(List<double> Bounds, List<double> Rates) : IDeskRecordStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskRecord item, double valueValue)
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

public sealed record DeskRecordDiscountOffsetAdjuster(double Weight) : IDeskRecordStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(DeskRecord item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record SourceDeskRecordAdjuster(Dictionary<string, double> Factors) : IDeskRecordStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(DeskRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDeskRecordAdjuster(double Percent) : IDeskRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the desk record steps in order and keeps a trace of each.</summary>
public sealed class DeskRecordEngine(List<IDeskRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskRecordEngine DefaultDeskRecordEngine()
    {
        return new DeskRecordEngine(
            new List<IDeskRecordStep>
            {
                new RoundingDeskRecordAdjuster(0.5),
                new TieredDeskRecordAdjuster(new List<double> { 5470.0, 8645.0 }, new List<double> { 0.99, 1.15 }),
                new DeskRecordDiscountOffsetAdjuster(0.05),
                new SourceDeskRecordAdjuster(new Dictionary<string, double> { ["north"] = 0.87 }),
                new PercentDeskRecordAdjuster(-8.0),
            },
            new List<string>(),
            0.4,
            31316.5);
    }

    public double Run(DeskRecord item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DeskRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
