namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh policy calculation.</summary>
public interface IRefreshPolicyAdjuster
{
    string Describe();
    double Adjust(RefreshPolicy item, double valueValue);
}

public sealed record TieredRefreshPolicyAdjuster(List<double> Bounds, List<double> Rates) : IRefreshPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshPolicy item, double valueValue)
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

public sealed record MinimumRefreshPolicyAdjuster(double Minimum) : IRefreshPolicyAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record UnitRefreshPolicyAdjuster(Dictionary<string, double> Factors) : IRefreshPolicyAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(RefreshPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RefreshPolicyUpdatedAtOffsetAdjuster(double Weight) : IRefreshPolicyAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(RefreshPolicy item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the refresh policy steps in order and keeps a trace of each.</summary>
public sealed class RefreshPolicyEngine(List<IRefreshPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshPolicyEngine DefaultRefreshPolicyEngine()
    {
        return new RefreshPolicyEngine(
            new List<IRefreshPolicyAdjuster>
            {
                new TieredRefreshPolicyAdjuster(new List<double> { 4817.0, 9986.0 }, new List<double> { 1.02, 0.97 }),
                new MinimumRefreshPolicyAdjuster(6.5),
                new UnitRefreshPolicyAdjuster(new Dictionary<string, double> { ["south"] = 1.16 }),
                new RefreshPolicyUpdatedAtOffsetAdjuster(0.88),
            },
            new List<string>(),
            3.8,
            55780.1);
    }

    public double Run(RefreshPolicy item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
