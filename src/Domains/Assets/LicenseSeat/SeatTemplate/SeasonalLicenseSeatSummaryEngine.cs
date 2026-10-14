namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal license seat summary calculation.</summary>
public interface ISeasonalLicenseSeatSummaryAdjuster
{
    string Describe();
    double Adjust(SeasonalLicenseSeatSummary item, double valueValue);
}

public sealed record MinimumSeasonalLicenseSeatSummaryAdjuster(double Minimum) : ISeasonalLicenseSeatSummaryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalLicenseSeatSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSeasonalLicenseSeatSummaryAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalLicenseSeatSummaryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalLicenseSeatSummary item, double valueValue)
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

public sealed record RoundingSeasonalLicenseSeatSummaryAdjuster(double Step) : ISeasonalLicenseSeatSummaryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalLicenseSeatSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSeasonalLicenseSeatSummaryAdjuster(double Cap) : ISeasonalLicenseSeatSummaryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalLicenseSeatSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal license seat summary steps in order and keeps a trace of each.</summary>
public sealed class SeasonalLicenseSeatSummaryEngine(List<ISeasonalLicenseSeatSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalLicenseSeatSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalLicenseSeatSummaryEngine DefaultSeasonalLicenseSeatSummaryEngine()
    {
        return new SeasonalLicenseSeatSummaryEngine(
            new List<ISeasonalLicenseSeatSummaryAdjuster>
            {
                new MinimumSeasonalLicenseSeatSummaryAdjuster(19.7),
                new TieredSeasonalLicenseSeatSummaryAdjuster(
                    new List<double> { 2341.0, 3512.0, 6828.0, 9732.0 },
                    new List<double> { 1.04, 1.18, 0.94, 0.91 }),
                new RoundingSeasonalLicenseSeatSummaryAdjuster(0.25),
                new CapSeasonalLicenseSeatSummaryAdjuster(49632.0),
            },
            new List<string>(),
            2.3,
            24950.1);
    }

    public double Compute(SeasonalLicenseSeatSummary item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalLicenseSeatSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
