namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment audit calculation.</summary>
public interface IAdjustmentAuditAdjuster
{
    string Describe();
    double Adjust(AdjustmentAudit item, double valueValue);
}

public sealed record TieredAdjustmentAuditAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentAudit item, double valueValue)
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

public sealed record AdjustmentAuditQuantityOffsetAdjuster(double Weight) : IAdjustmentAuditAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(AdjustmentAudit item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record CapAdjustmentAuditAdjuster(double Cap) : IAdjustmentAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAdjustmentAuditAdjuster(double Step) : IAdjustmentAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdjustmentAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adjustment audit steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentAuditEngine(List<IAdjustmentAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentAuditEngine DefaultAdjustmentAuditEngine()
    {
        return new AdjustmentAuditEngine(
            new List<IAdjustmentAuditAdjuster>
            {
                new TieredAdjustmentAuditAdjuster(
                    new List<double> { 637.0, 1974.0, 7582.0 },
                    new List<double> { 1.01, 0.98, 1.02 }),
                new AdjustmentAuditQuantityOffsetAdjuster(1.78),
                new CapAdjustmentAuditAdjuster(9247.0),
                new RoundingAdjustmentAuditAdjuster(1.0),
            },
            new List<string>(),
            0.1,
            52688.4);
    }

    public double Price(AdjustmentAudit item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
