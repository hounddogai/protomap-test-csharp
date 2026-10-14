namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>One step of the regional pay run batch calculation.</summary>
public interface IRegionalPayRunBatchStep
{
    string Describe();
    double Adjust(RegionalPayRunBatch item, double valueValue);
}

public sealed record CapRegionalPayRunBatchAdjuster(double Cap) : IRegionalPayRunBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalPayRunBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryRegionalPayRunBatchAdjuster(Dictionary<string, double> Factors) : IRegionalPayRunBatchStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RegionalPayRunBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RegionalPayRunBatchVersionOffsetAdjuster(double Weight) : IRegionalPayRunBatchStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(RegionalPayRunBatch item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

/// <summary>Runs the regional pay run batch steps in order and keeps a trace of each.</summary>
public sealed class RegionalPayRunBatchEngine(List<IRegionalPayRunBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalPayRunBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalPayRunBatchEngine DefaultRegionalPayRunBatchEngine()
    {
        return new RegionalPayRunBatchEngine(
            new List<IRegionalPayRunBatchStep>
            {
                new CapRegionalPayRunBatchAdjuster(45640.0),
                new CountryRegionalPayRunBatchAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.18, ["north"] = 1.29, ["south"] = 0.76 }),
                new RegionalPayRunBatchVersionOffsetAdjuster(1.61),
            },
            new List<string>(),
            3.1,
            15368.2);
    }

    public double Evaluate(RegionalPayRunBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalPayRunBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
