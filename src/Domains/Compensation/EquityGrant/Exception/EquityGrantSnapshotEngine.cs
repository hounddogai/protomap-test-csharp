namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant snapshot calculation.</summary>
public interface IEquityGrantSnapshotModifier
{
    string Describe();
    double Adjust(EquityGrantSnapshot item, double valueValue);
}

public sealed record CapEquityGrantSnapshotAdjuster(double Cap) : IEquityGrantSnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumEquityGrantSnapshotAdjuster(double Minimum) : IEquityGrantSnapshotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EquityGrantSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelEquityGrantSnapshotAdjuster(Dictionary<string, double> Factors) : IEquityGrantSnapshotModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(EquityGrantSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredEquityGrantSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantSnapshotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantSnapshot item, double valueValue)
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

/// <summary>Runs the equity grant snapshot steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantSnapshotEngine(List<IEquityGrantSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantSnapshotEngine DefaultEquityGrantSnapshotEngine()
    {
        return new EquityGrantSnapshotEngine(
            new List<IEquityGrantSnapshotModifier>
            {
                new CapEquityGrantSnapshotAdjuster(48435.0),
                new MinimumEquityGrantSnapshotAdjuster(42.9),
                new LabelEquityGrantSnapshotAdjuster(new Dictionary<string, double> { ["east"] = 1.01 }),
                new TieredEquityGrantSnapshotAdjuster(
                    new List<double> { 454.0, 493.0, 9154.0 },
                    new List<double> { 0.98, 1.06, 1.03 }),
            },
            new List<string>(),
            1.4,
            51479.2);
    }

    public double Evaluate(EquityGrantSnapshot item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
