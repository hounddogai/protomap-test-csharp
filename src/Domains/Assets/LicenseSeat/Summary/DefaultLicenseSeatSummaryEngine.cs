namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default license seat summary calculation.</summary>
public interface IDefaultLicenseSeatSummaryStep
{
    string Describe();
    double Adjust(DefaultLicenseSeatSummary item, double valueValue);
}

public sealed record PercentDefaultLicenseSeatSummaryAdjuster(double Percent) : IDefaultLicenseSeatSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultLicenseSeatSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultLicenseSeatSummaryAdjuster(List<double> Bounds, List<double> Rates) : IDefaultLicenseSeatSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultLicenseSeatSummary item, double valueValue)
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

public sealed record RoundingDefaultLicenseSeatSummaryAdjuster(double Step) : IDefaultLicenseSeatSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultLicenseSeatSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentDefaultLicenseSeatSummaryAdjuster(Dictionary<string, double> Factors) : IDefaultLicenseSeatSummaryStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(DefaultLicenseSeatSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the default license seat summary steps in order and keeps a trace of each.</summary>
public sealed class DefaultLicenseSeatSummaryEngine(List<IDefaultLicenseSeatSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultLicenseSeatSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultLicenseSeatSummaryEngine DefaultDefaultLicenseSeatSummaryEngine()
    {
        return new DefaultLicenseSeatSummaryEngine(
            new List<IDefaultLicenseSeatSummaryStep>
            {
                new PercentDefaultLicenseSeatSummaryAdjuster(-22.7),
                new TieredDefaultLicenseSeatSummaryAdjuster(
                    new List<double> { 2581.0, 7485.0 },
                    new List<double> { 0.95, 1.05 }),
                new RoundingDefaultLicenseSeatSummaryAdjuster(5.0),
                new SegmentDefaultLicenseSeatSummaryAdjuster(new Dictionary<string, double> { ["east"] = 1.38 }),
            },
            new List<string>(),
            4.0,
            10886.2);
    }

    public double Price(DefaultLicenseSeatSummary item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultLicenseSeatSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
