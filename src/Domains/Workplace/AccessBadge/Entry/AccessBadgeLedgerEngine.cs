namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge ledger calculation.</summary>
public interface IAccessBadgeLedgerModifier
{
    string Describe();
    double Adjust(AccessBadgeLedger item, double valueValue);
}

public sealed record PercentAccessBadgeLedgerAdjuster(double Percent) : IAccessBadgeLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAccessBadgeLedgerAdjuster(double Minimum) : IAccessBadgeLedgerModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAccessBadgeLedgerAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeLedgerModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeLedger item, double valueValue)
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

/// <summary>Runs the access badge ledger steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeLedgerEngine(List<IAccessBadgeLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeLedgerEngine DefaultAccessBadgeLedgerEngine()
    {
        return new AccessBadgeLedgerEngine(
            new List<IAccessBadgeLedgerModifier>
            {
                new PercentAccessBadgeLedgerAdjuster(2.8),
                new MinimumAccessBadgeLedgerAdjuster(0.1),
                new TieredAccessBadgeLedgerAdjuster(
                    new List<double> { 2237.0, 5875.0, 7869.0 },
                    new List<double> { 0.91, 0.93, 1.17 }),
            },
            new List<string>(),
            4.9,
            17471.3);
    }

    public double Run(AccessBadgeLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
