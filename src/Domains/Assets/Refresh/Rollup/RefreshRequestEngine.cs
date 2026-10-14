namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh request calculation.</summary>
public interface IRefreshRequestModifier
{
    string Describe();
    double Adjust(RefreshRequest item, double valueValue);
}

public sealed record CapRefreshRequestAdjuster(double Cap) : IRefreshRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleRefreshRequestAdjuster(Dictionary<string, double> Factors) : IRefreshRequestModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RefreshRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRefreshRequestAdjuster(double Percent) : IRefreshRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRefreshRequestAdjuster(double Minimum) : IRefreshRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the refresh request steps in order and keeps a trace of each.</summary>
public sealed class RefreshRequestEngine(List<IRefreshRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshRequestEngine DefaultRefreshRequestEngine()
    {
        return new RefreshRequestEngine(
            new List<IRefreshRequestModifier>
            {
                new CapRefreshRequestAdjuster(5304.0),
                new TitleRefreshRequestAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.74, ["central"] = 1.27 }),
                new PercentRefreshRequestAdjuster(11.6),
                new MinimumRefreshRequestAdjuster(49.1),
            },
            new List<string>(),
            3.5,
            57756.2);
    }

    public double Price(RefreshRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<RefreshRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
