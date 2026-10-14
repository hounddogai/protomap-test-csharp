namespace Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>One step of the seasonal garnishment notice calculation.</summary>
public interface ISeasonalGarnishmentNoticeModifier
{
    string Describe();
    double Adjust(SeasonalGarnishmentNotice item, double valueValue);
}

public sealed record TieredSeasonalGarnishmentNoticeAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalGarnishmentNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalGarnishmentNotice item, double valueValue)
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

public sealed record CapSeasonalGarnishmentNoticeAdjuster(double Cap) : ISeasonalGarnishmentNoticeModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalGarnishmentNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SeasonalGarnishmentNoticeTenureMonthsOffsetAdjuster(double Weight) : ISeasonalGarnishmentNoticeModifier
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(SeasonalGarnishmentNotice item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record PercentSeasonalGarnishmentNoticeAdjuster(double Percent) : ISeasonalGarnishmentNoticeModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalGarnishmentNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal garnishment notice steps in order and keeps a trace of each.</summary>
public sealed class SeasonalGarnishmentNoticeEngine(List<ISeasonalGarnishmentNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalGarnishmentNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalGarnishmentNoticeEngine DefaultSeasonalGarnishmentNoticeEngine()
    {
        return new SeasonalGarnishmentNoticeEngine(
            new List<ISeasonalGarnishmentNoticeModifier>
            {
                new TieredSeasonalGarnishmentNoticeAdjuster(
                    new List<double> { 5103.0, 5115.0, 8226.0, 9752.0 },
                    new List<double> { 0.88, 0.96, 1.16, 1.02 }),
                new CapSeasonalGarnishmentNoticeAdjuster(23117.0),
                new SeasonalGarnishmentNoticeTenureMonthsOffsetAdjuster(0.5),
                new PercentSeasonalGarnishmentNoticeAdjuster(-15.2),
            },
            new List<string>(),
            3.0,
            52618.1);
    }

    public double Compute(SeasonalGarnishmentNotice item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalGarnishmentNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
