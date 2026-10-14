namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal tax form tier calculation.</summary>
public interface ISeasonalTaxFormTierStep
{
    string Describe();
    double Adjust(SeasonalTaxFormTier item, double valueValue);
}

public sealed record FlaggedSeasonalTaxFormTierAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalTaxFormTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalTaxFormTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSeasonalTaxFormTierAdjuster(double Percent) : ISeasonalTaxFormTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalTaxFormTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSeasonalTaxFormTierAdjuster(double Step) : ISeasonalTaxFormTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalTaxFormTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamSeasonalTaxFormTierAdjuster(Dictionary<string, double> Factors) : ISeasonalTaxFormTierStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SeasonalTaxFormTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the seasonal tax form tier steps in order and keeps a trace of each.</summary>
public sealed class SeasonalTaxFormTierEngine(List<ISeasonalTaxFormTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalTaxFormTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalTaxFormTierEngine DefaultSeasonalTaxFormTierEngine(Registry flags)
    {
        return new SeasonalTaxFormTierEngine(
            new List<ISeasonalTaxFormTierStep>
            {
                new FlaggedSeasonalTaxFormTierAdjuster(flags, "seasonal-tax-form-tier-legacy", 8.5),
                new PercentSeasonalTaxFormTierAdjuster(-1.1),
                new RoundingSeasonalTaxFormTierAdjuster(0.1),
                new TeamSeasonalTaxFormTierAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.16, ["east"] = 1.17, ["south"] = 1.35 }),
            },
            new List<string>(),
            2.3,
            4602.7);
    }

    public double Run(SeasonalTaxFormTier item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalTaxFormTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
