namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict audit record calculation.</summary>
public interface IStrictAuditRecordFactor
{
    string Describe();
    double Adjust(StrictAuditRecord item, double valueValue);
}

public sealed record CapStrictAuditRecordAdjuster(double Cap) : IStrictAuditRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictAuditRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record StrictAuditRecordCostOffsetAdjuster(double Weight) : IStrictAuditRecordFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(StrictAuditRecord item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record TitleStrictAuditRecordAdjuster(Dictionary<string, double> Factors) : IStrictAuditRecordFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(StrictAuditRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingStrictAuditRecordAdjuster(double Step) : IStrictAuditRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictAuditRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict audit record steps in order and keeps a trace of each.</summary>
public sealed class StrictAuditRecordEngine(List<IStrictAuditRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAuditRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAuditRecordEngine DefaultStrictAuditRecordEngine()
    {
        return new StrictAuditRecordEngine(
            new List<IStrictAuditRecordFactor>
            {
                new CapStrictAuditRecordAdjuster(35745.0),
                new StrictAuditRecordCostOffsetAdjuster(0.96),
                new TitleStrictAuditRecordAdjuster(new Dictionary<string, double> { ["west"] = 0.9 }),
                new RoundingStrictAuditRecordAdjuster(1.0),
            },
            new List<string>(),
            1.6,
            99427.1);
    }

    public double Compute(StrictAuditRecord item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StrictAuditRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
