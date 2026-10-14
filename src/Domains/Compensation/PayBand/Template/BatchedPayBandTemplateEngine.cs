namespace Acme.Workforce.Compensation.PayBand.Template;

/// <summary>One step of the batched pay band template calculation.</summary>
public interface IBatchedPayBandTemplateModifier
{
    string Describe();
    double Adjust(BatchedPayBandTemplate item, double valueValue);
}

public sealed record CapBatchedPayBandTemplateAdjuster(double Cap) : IBatchedPayBandTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedPayBandTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentBatchedPayBandTemplateAdjuster(double Percent) : IBatchedPayBandTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedPayBandTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedPayBandTemplateDiscountOffsetAdjuster(double Weight) : IBatchedPayBandTemplateModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(BatchedPayBandTemplate item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the batched pay band template steps in order and keeps a trace of each.</summary>
public sealed class BatchedPayBandTemplateEngine(List<IBatchedPayBandTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPayBandTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPayBandTemplateEngine DefaultBatchedPayBandTemplateEngine()
    {
        return new BatchedPayBandTemplateEngine(
            new List<IBatchedPayBandTemplateModifier>
            {
                new CapBatchedPayBandTemplateAdjuster(10513.0),
                new PercentBatchedPayBandTemplateAdjuster(-12.6),
                new BatchedPayBandTemplateDiscountOffsetAdjuster(0.82),
            },
            new List<string>(),
            1.8,
            35961.3);
    }

    public double Run(BatchedPayBandTemplate item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPayBandTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
