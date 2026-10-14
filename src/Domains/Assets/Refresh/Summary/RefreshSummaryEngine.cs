namespace Acme.Workforce.Assets.Refresh.Summary;

/// <summary>One step of the refresh summary calculation.</summary>
public interface IRefreshSummaryFactor
{
    string Describe();
    double Adjust(RefreshSummary item, double valueValue);
}

public sealed record PercentRefreshSummaryAdjuster(double Percent) : IRefreshSummaryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleRefreshSummaryAdjuster(Dictionary<string, double> Factors) : IRefreshSummaryFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(RefreshSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRefreshSummaryAdjuster(double Minimum) : IRefreshSummaryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the refresh summary steps in order and keeps a trace of each.</summary>
public sealed class RefreshSummaryEngine(List<IRefreshSummaryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshSummaryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshSummaryEngine DefaultRefreshSummaryEngine()
    {
        return new RefreshSummaryEngine(
            new List<IRefreshSummaryFactor>
            {
                new PercentRefreshSummaryAdjuster(-11.6),
                new LocaleRefreshSummaryAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.04, ["central"] = 1.21 }),
                new MinimumRefreshSummaryAdjuster(1.2),
            },
            new List<string>(),
            2.0,
            7850.7);
    }

    public double Price(RefreshSummary item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
