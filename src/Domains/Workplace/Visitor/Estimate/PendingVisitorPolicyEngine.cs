namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending visitor policy calculation.</summary>
public interface IPendingVisitorPolicyModifier
{
    string Describe();
    double Adjust(PendingVisitorPolicy item, double valueValue);
}

public sealed record CapPendingVisitorPolicyAdjuster(double Cap) : IPendingVisitorPolicyModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingVisitorPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPendingVisitorPolicyAdjuster(List<double> Bounds, List<double> Rates) : IPendingVisitorPolicyModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingVisitorPolicy item, double valueValue)
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

public sealed record RoundingPendingVisitorPolicyAdjuster(double Step) : IPendingVisitorPolicyModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingVisitorPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending visitor policy steps in order and keeps a trace of each.</summary>
public sealed class PendingVisitorPolicyEngine(List<IPendingVisitorPolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingVisitorPolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingVisitorPolicyEngine DefaultPendingVisitorPolicyEngine()
    {
        return new PendingVisitorPolicyEngine(
            new List<IPendingVisitorPolicyModifier>
            {
                new CapPendingVisitorPolicyAdjuster(29721.0),
                new TieredPendingVisitorPolicyAdjuster(
                    new List<double> { 3404.0, 5929.0, 5960.0, 6996.0 },
                    new List<double> { 0.95, 0.99, 1.14, 1.01 }),
                new RoundingPendingVisitorPolicyAdjuster(0.05),
            },
            new List<string>(),
            2.6,
            73825.4);
    }

    public double Evaluate(PendingVisitorPolicy item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PendingVisitorPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
