namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh score calculation.</summary>
public interface IRefreshScoreModifier
{
    string Describe();
    double Adjust(RefreshScore item, double valueValue);
}

public sealed record RefreshScoreRatioOffsetAdjuster(double Weight) : IRefreshScoreModifier
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(RefreshScore item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record CategoryRefreshScoreAdjuster(Dictionary<string, double> Factors) : IRefreshScoreModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(RefreshScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRefreshScoreAdjuster(double Minimum) : IRefreshScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRefreshScoreAdjuster(double Cap) : IRefreshScoreModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRefreshScoreAdjuster(double Percent) : IRefreshScoreModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRefreshScoreAdjuster(List<double> Bounds, List<double> Rates) : IRefreshScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshScore item, double valueValue)
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

/// <summary>Runs the refresh score steps in order and keeps a trace of each.</summary>
public sealed class RefreshScoreEngine(List<IRefreshScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshScoreEngine DefaultRefreshScoreEngine()
    {
        return new RefreshScoreEngine(
            new List<IRefreshScoreModifier>
            {
                new RefreshScoreRatioOffsetAdjuster(1.13),
                new CategoryRefreshScoreAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.73, ["north"] = 1.28, ["online"] = 1.18 }),
                new MinimumRefreshScoreAdjuster(0.8),
                new CapRefreshScoreAdjuster(10735.0),
                new PercentRefreshScoreAdjuster(-0.3),
                new TieredRefreshScoreAdjuster(new List<double> { 2328.0, 2965.0 }, new List<double> { 0.91, 0.9 }),
            },
            new List<string>(),
            0.6,
            96736.3);
    }

    public double Evaluate(RefreshScore item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
