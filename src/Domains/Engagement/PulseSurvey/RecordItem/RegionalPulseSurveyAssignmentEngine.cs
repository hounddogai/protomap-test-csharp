namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional pulse survey assignment calculation.</summary>
public interface IRegionalPulseSurveyAssignmentAdjuster
{
    string Describe();
    double Adjust(RegionalPulseSurveyAssignment item, double valueValue);
}

public sealed record PercentRegionalPulseSurveyAssignmentAdjuster(double Percent) : IRegionalPulseSurveyAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalPulseSurveyAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRegionalPulseSurveyAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IRegionalPulseSurveyAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalPulseSurveyAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionalPulseSurveyAssignmentBatchSizeOffsetAdjuster(double Weight) : IRegionalPulseSurveyAssignmentAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(RegionalPulseSurveyAssignment item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record TieredRegionalPulseSurveyAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IRegionalPulseSurveyAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalPulseSurveyAssignment item, double valueValue)
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

public sealed record RoundingRegionalPulseSurveyAssignmentAdjuster(double Step) : IRegionalPulseSurveyAssignmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalPulseSurveyAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelRegionalPulseSurveyAssignmentAdjuster(Dictionary<string, double> Factors) : IRegionalPulseSurveyAssignmentAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RegionalPulseSurveyAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the regional pulse survey assignment steps in order and keeps a trace of each.</summary>
public sealed class RegionalPulseSurveyAssignmentEngine(List<IRegionalPulseSurveyAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalPulseSurveyAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalPulseSurveyAssignmentEngine DefaultRegionalPulseSurveyAssignmentEngine(Registry flags)
    {
        return new RegionalPulseSurveyAssignmentEngine(
            new List<IRegionalPulseSurveyAssignmentAdjuster>
            {
                new PercentRegionalPulseSurveyAssignmentAdjuster(7.4),
                new FlaggedRegionalPulseSurveyAssignmentAdjuster(
                    flags,
                    "regional-pulse-survey-assignment-regional",
                    -4.6),
                new RegionalPulseSurveyAssignmentBatchSizeOffsetAdjuster(1.02),
                new TieredRegionalPulseSurveyAssignmentAdjuster(
                    new List<double> { 1754.0, 4786.0, 4957.0, 5295.0 },
                    new List<double> { 0.85, 0.98, 1.09, 1.09 }),
                new RoundingRegionalPulseSurveyAssignmentAdjuster(1.0),
                new LabelRegionalPulseSurveyAssignmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.38, ["east"] = 0.72 }),
            },
            new List<string>(),
            3.7,
            97876.3);
    }

    public double Run(RegionalPulseSurveyAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalPulseSurveyAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
