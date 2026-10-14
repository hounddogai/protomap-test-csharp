namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh override calculation.</summary>
public interface IRefreshOverrideStep
{
    string Describe();
    double Adjust(RefreshOverride item, double valueValue);
}

public sealed record LabelRefreshOverrideAdjuster(Dictionary<string, double> Factors) : IRefreshOverrideStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RefreshOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRefreshOverrideAdjuster(double Step) : IRefreshOverrideStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRefreshOverrideAdjuster(double Cap) : IRefreshOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRefreshOverrideAdjuster(double Percent) : IRefreshOverrideStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RefreshOverrideMarginOffsetAdjuster(double Weight) : IRefreshOverrideStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(RefreshOverride item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record MinimumRefreshOverrideAdjuster(double Minimum) : IRefreshOverrideStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the refresh override steps in order and keeps a trace of each.</summary>
public sealed class RefreshOverrideEngine(List<IRefreshOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshOverrideEngine DefaultRefreshOverrideEngine()
    {
        return new RefreshOverrideEngine(
            new List<IRefreshOverrideStep>
            {
                new LabelRefreshOverrideAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.29, ["east"] = 0.78, ["north"] = 1.15 }),
                new RoundingRefreshOverrideAdjuster(0.25),
                new CapRefreshOverrideAdjuster(32023.0),
                new PercentRefreshOverrideAdjuster(-10.5),
                new RefreshOverrideMarginOffsetAdjuster(0.52),
                new MinimumRefreshOverrideAdjuster(27.8),
            },
            new List<string>(),
            4.4,
            32144.9);
    }

    public double Compute(RefreshOverride item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
