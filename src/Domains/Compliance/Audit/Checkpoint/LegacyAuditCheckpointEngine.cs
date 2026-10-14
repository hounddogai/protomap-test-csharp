namespace Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>One step of the legacy audit checkpoint calculation.</summary>
public interface ILegacyAuditCheckpointModifier
{
    string Describe();
    double Adjust(LegacyAuditCheckpoint item, double valueValue);
}

public sealed record TieredLegacyAuditCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ILegacyAuditCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyAuditCheckpoint item, double valueValue)
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

public sealed record LegacyAuditCheckpointQuantityOffsetAdjuster(double Weight) : ILegacyAuditCheckpointModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(LegacyAuditCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record CapLegacyAuditCheckpointAdjuster(double Cap) : ILegacyAuditCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyAuditCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy audit checkpoint steps in order and keeps a trace of each.</summary>
public sealed class LegacyAuditCheckpointEngine(List<ILegacyAuditCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyAuditCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyAuditCheckpointEngine DefaultLegacyAuditCheckpointEngine()
    {
        return new LegacyAuditCheckpointEngine(
            new List<ILegacyAuditCheckpointModifier>
            {
                new TieredLegacyAuditCheckpointAdjuster(
                    new List<double> { 1454.0, 2360.0, 6785.0, 7630.0 },
                    new List<double> { 0.8, 1.06, 0.94, 1.16 }),
                new LegacyAuditCheckpointQuantityOffsetAdjuster(0.49),
                new CapLegacyAuditCheckpointAdjuster(38186.0),
            },
            new List<string>(),
            3.5,
            32021.4);
    }

    public double Evaluate(LegacyAuditCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyAuditCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
