namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal refresh window calculation.</summary>
public interface ISeasonalRefreshWindowModifier
{
    string Describe();
    double Adjust(SeasonalRefreshWindow item, double valueValue);
}

public sealed record SeasonalRefreshWindowMaximumOffsetAdjuster(double Weight) : ISeasonalRefreshWindowModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(SeasonalRefreshWindow item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record TitleSeasonalRefreshWindowAdjuster(Dictionary<string, double> Factors) : ISeasonalRefreshWindowModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SeasonalRefreshWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSeasonalRefreshWindowAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalRefreshWindowModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalRefreshWindow item, double valueValue)
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

public sealed record RoundingSeasonalRefreshWindowAdjuster(double Step) : ISeasonalRefreshWindowModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalRefreshWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the seasonal refresh window steps in order and keeps a trace of each.</summary>
public sealed class SeasonalRefreshWindowEngine(List<ISeasonalRefreshWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalRefreshWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalRefreshWindowEngine DefaultSeasonalRefreshWindowEngine()
    {
        return new SeasonalRefreshWindowEngine(
            new List<ISeasonalRefreshWindowModifier>
            {
                new SeasonalRefreshWindowMaximumOffsetAdjuster(0.73),
                new TitleSeasonalRefreshWindowAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.95, ["west"] = 1.23, ["north"] = 0.77 }),
                new TieredSeasonalRefreshWindowAdjuster(
                    new List<double> { 1227.0, 3277.0, 3812.0 },
                    new List<double> { 0.88, 1.15, 1.12 }),
                new RoundingSeasonalRefreshWindowAdjuster(0.5),
            },
            new List<string>(),
            3.6,
            76559.4);
    }

    public double Price(SeasonalRefreshWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalRefreshWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
