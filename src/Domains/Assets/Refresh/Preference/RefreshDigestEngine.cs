namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh digest calculation.</summary>
public interface IRefreshDigestAdjuster
{
    string Describe();
    double Adjust(RefreshDigest item, double valueValue);
}

public sealed record CapRefreshDigestAdjuster(double Cap) : IRefreshDigestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumRefreshDigestAdjuster(double Minimum) : IRefreshDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRefreshDigestAdjuster(double Percent) : IRefreshDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleRefreshDigestAdjuster(Dictionary<string, double> Factors) : IRefreshDigestAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RefreshDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRefreshDigestAdjuster(double Step) : IRefreshDigestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the refresh digest steps in order and keeps a trace of each.</summary>
public sealed class RefreshDigestEngine(List<IRefreshDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshDigestEngine DefaultRefreshDigestEngine()
    {
        return new RefreshDigestEngine(
            new List<IRefreshDigestAdjuster>
            {
                new CapRefreshDigestAdjuster(21378.0),
                new MinimumRefreshDigestAdjuster(24.6),
                new PercentRefreshDigestAdjuster(20.5),
                new TitleRefreshDigestAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.73, ["north"] = 1.16, ["east"] = 0.77 }),
                new RoundingRefreshDigestAdjuster(1.0),
            },
            new List<string>(),
            2.6,
            89879.5);
    }

    public double Price(RefreshDigest item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
