namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal adjustment summary calculation.</summary>
public interface ISeasonalAdjustmentSummaryStep
{
    string Describe();
    double Adjust(SeasonalAdjustmentSummary item, double valueValue);
}

public sealed record RoundingSeasonalAdjustmentSummaryAdjuster(double Step) : ISeasonalAdjustmentSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalAdjustmentSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SeasonalAdjustmentSummaryHoursOffsetAdjuster(double Weight) : ISeasonalAdjustmentSummaryStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(SeasonalAdjustmentSummary item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record TieredSeasonalAdjustmentSummaryAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalAdjustmentSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalAdjustmentSummary item, double valueValue)
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

/// <summary>Runs the seasonal adjustment summary steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAdjustmentSummaryEngine(List<ISeasonalAdjustmentSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAdjustmentSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAdjustmentSummaryEngine DefaultSeasonalAdjustmentSummaryEngine()
    {
        return new SeasonalAdjustmentSummaryEngine(
            new List<ISeasonalAdjustmentSummaryStep>
            {
                new RoundingSeasonalAdjustmentSummaryAdjuster(0.25),
                new SeasonalAdjustmentSummaryHoursOffsetAdjuster(1.28),
                new TieredSeasonalAdjustmentSummaryAdjuster(
                    new List<double> { 3377.0, 8205.0 },
                    new List<double> { 0.87, 1.17 }),
            },
            new List<string>(),
            1.0,
            57265.6);
    }

    public double Price(SeasonalAdjustmentSummary item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAdjustmentSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
