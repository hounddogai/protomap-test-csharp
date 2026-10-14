namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual pulse survey summary calculation.</summary>
public interface IManualPulseSurveySummaryModifier
{
    string Describe();
    double Adjust(ManualPulseSurveySummary item, double valueValue);
}

public sealed record UnitManualPulseSurveySummaryAdjuster(Dictionary<string, double> Factors) : IManualPulseSurveySummaryModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(ManualPulseSurveySummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedManualPulseSurveySummaryAdjuster(Registry Flags, string Flag, double Percent) : IManualPulseSurveySummaryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualPulseSurveySummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ManualPulseSurveySummaryUpdatedAtOffsetAdjuster(double Weight) : IManualPulseSurveySummaryModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(ManualPulseSurveySummary item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record TieredManualPulseSurveySummaryAdjuster(List<double> Bounds, List<double> Rates) : IManualPulseSurveySummaryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualPulseSurveySummary item, double valueValue)
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

public sealed record RoundingManualPulseSurveySummaryAdjuster(double Step) : IManualPulseSurveySummaryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualPulseSurveySummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the manual pulse survey summary steps in order and keeps a trace of each.</summary>
public sealed class ManualPulseSurveySummaryEngine(List<IManualPulseSurveySummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualPulseSurveySummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualPulseSurveySummaryEngine DefaultManualPulseSurveySummaryEngine(Registry flags)
    {
        return new ManualPulseSurveySummaryEngine(
            new List<IManualPulseSurveySummaryModifier>
            {
                new UnitManualPulseSurveySummaryAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.9, ["central"] = 0.84 }),
                new FlaggedManualPulseSurveySummaryAdjuster(flags, "manual-pulse-survey-summary-pending", 4.1),
                new ManualPulseSurveySummaryUpdatedAtOffsetAdjuster(0.13),
                new TieredManualPulseSurveySummaryAdjuster(
                    new List<double> { 1090.0, 9285.0 },
                    new List<double> { 0.96, 0.89 }),
                new RoundingManualPulseSurveySummaryAdjuster(0.25),
            },
            new List<string>(),
            0.5,
            11860.1);
    }

    public double Compute(ManualPulseSurveySummary item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualPulseSurveySummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
