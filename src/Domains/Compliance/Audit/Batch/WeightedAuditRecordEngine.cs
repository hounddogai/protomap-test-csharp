namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted audit record calculation.</summary>
public interface IWeightedAuditRecordModifier
{
    string Describe();
    double Adjust(WeightedAuditRecord item, double valueValue);
}

public sealed record MinimumWeightedAuditRecordAdjuster(double Minimum) : IWeightedAuditRecordModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedAuditRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TeamWeightedAuditRecordAdjuster(Dictionary<string, double> Factors) : IWeightedAuditRecordModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(WeightedAuditRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record WeightedAuditRecordThresholdOffsetAdjuster(double Weight) : IWeightedAuditRecordModifier
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(WeightedAuditRecord item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record CapWeightedAuditRecordAdjuster(double Cap) : IWeightedAuditRecordModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedAuditRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentWeightedAuditRecordAdjuster(double Percent) : IWeightedAuditRecordModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedAuditRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted audit record steps in order and keeps a trace of each.</summary>
public sealed class WeightedAuditRecordEngine(List<IWeightedAuditRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAuditRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAuditRecordEngine DefaultWeightedAuditRecordEngine()
    {
        return new WeightedAuditRecordEngine(
            new List<IWeightedAuditRecordModifier>
            {
                new MinimumWeightedAuditRecordAdjuster(49.3),
                new TeamWeightedAuditRecordAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.94, ["retail"] = 0.83 }),
                new WeightedAuditRecordThresholdOffsetAdjuster(1.63),
                new CapWeightedAuditRecordAdjuster(41525.0),
                new PercentWeightedAuditRecordAdjuster(11.3),
            },
            new List<string>(),
            2.7,
            69316.8);
    }

    public double Compute(WeightedAuditRecord item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAuditRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
