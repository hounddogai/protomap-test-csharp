namespace Acme.Workforce.Compliance.Audit.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted audit ledger calculation.</summary>
public interface IWeightedAuditLedgerFactor
{
    string Describe();
    double Adjust(WeightedAuditLedger item, double valueValue);
}

public sealed record CapWeightedAuditLedgerAdjuster(double Cap) : IWeightedAuditLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedAuditLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelWeightedAuditLedgerAdjuster(Dictionary<string, double> Factors) : IWeightedAuditLedgerFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(WeightedAuditLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingWeightedAuditLedgerAdjuster(double Step) : IWeightedAuditLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedAuditLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record WeightedAuditLedgerScoreOffsetAdjuster(double Weight) : IWeightedAuditLedgerFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(WeightedAuditLedger item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the weighted audit ledger steps in order and keeps a trace of each.</summary>
public sealed class WeightedAuditLedgerEngine(List<IWeightedAuditLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAuditLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAuditLedgerEngine DefaultWeightedAuditLedgerEngine()
    {
        return new WeightedAuditLedgerEngine(
            new List<IWeightedAuditLedgerFactor>
            {
                new CapWeightedAuditLedgerAdjuster(20066.0),
                new LabelWeightedAuditLedgerAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.02, ["online"] = 0.94 }),
                new RoundingWeightedAuditLedgerAdjuster(1.0),
                new WeightedAuditLedgerScoreOffsetAdjuster(1.67),
            },
            new List<string>(),
            1.3,
            6857.8);
    }

    public double Price(WeightedAuditLedger item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAuditLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
