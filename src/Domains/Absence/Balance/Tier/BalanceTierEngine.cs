namespace Acme.Workforce.Absence.Balance.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance tier calculation.</summary>
public interface IBalanceTierAdjuster
{
    string Describe();
    double Adjust(BalanceTier item, double valueValue);
}

public sealed record LabelBalanceTierAdjuster(Dictionary<string, double> Factors) : IBalanceTierAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BalanceTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapBalanceTierAdjuster(double Cap) : IBalanceTierAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BalanceTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumBalanceTierAdjuster(double Minimum) : IBalanceTierAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record BalanceTierDurationMinutesOffsetAdjuster(double Weight) : IBalanceTierAdjuster
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(BalanceTier item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record PercentBalanceTierAdjuster(double Percent) : IBalanceTierAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBalanceTierAdjuster(List<double> Bounds, List<double> Rates) : IBalanceTierAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BalanceTier item, double valueValue)
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

/// <summary>Runs the balance tier steps in order and keeps a trace of each.</summary>
public sealed class BalanceTierEngine(List<IBalanceTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceTierEngine DefaultBalanceTierEngine()
    {
        return new BalanceTierEngine(
            new List<IBalanceTierAdjuster>
            {
                new LabelBalanceTierAdjuster(new Dictionary<string, double> { ["west"] = 0.81 }),
                new CapBalanceTierAdjuster(26459.0),
                new MinimumBalanceTierAdjuster(16.4),
                new BalanceTierDurationMinutesOffsetAdjuster(1.86),
                new PercentBalanceTierAdjuster(-5.8),
                new TieredBalanceTierAdjuster(
                    new List<double> { 1732.0, 6630.0, 8831.0, 8961.0 },
                    new List<double> { 1.11, 0.85, 1.17, 1.01 }),
            },
            new List<string>(),
            2.2,
            64227.1);
    }

    public double Compute(BalanceTier item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
