namespace Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>One step of the batched headcount template calculation.</summary>
public interface IBatchedHeadcountTemplateFactor
{
    string Describe();
    double Adjust(BatchedHeadcountTemplate item, double valueValue);
}

public sealed record UnitBatchedHeadcountTemplateAdjuster(Dictionary<string, double> Factors) : IBatchedHeadcountTemplateFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(BatchedHeadcountTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapBatchedHeadcountTemplateAdjuster(double Cap) : IBatchedHeadcountTemplateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedHeadcountTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record BatchedHeadcountTemplateQuantityOffsetAdjuster(double Weight) : IBatchedHeadcountTemplateFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(BatchedHeadcountTemplate item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the batched headcount template steps in order and keeps a trace of each.</summary>
public sealed class BatchedHeadcountTemplateEngine(List<IBatchedHeadcountTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedHeadcountTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedHeadcountTemplateEngine DefaultBatchedHeadcountTemplateEngine()
    {
        return new BatchedHeadcountTemplateEngine(
            new List<IBatchedHeadcountTemplateFactor>
            {
                new UnitBatchedHeadcountTemplateAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.04, ["south"] = 1.29 }),
                new CapBatchedHeadcountTemplateAdjuster(41487.0),
                new BatchedHeadcountTemplateQuantityOffsetAdjuster(1.86),
            },
            new List<string>(),
            1.1,
            82940.4);
    }

    public double Compute(BatchedHeadcountTemplate item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedHeadcountTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
