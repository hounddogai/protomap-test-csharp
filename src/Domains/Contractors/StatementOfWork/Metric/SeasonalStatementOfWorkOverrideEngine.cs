namespace Acme.Workforce.Contractors.StatementOfWork.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal statement of work override calculation.</summary>
public interface ISeasonalStatementOfWorkOverrideAdjuster
{
    string Describe();
    double Adjust(SeasonalStatementOfWorkOverride item, double valueValue);
}

public sealed record TitleSeasonalStatementOfWorkOverrideAdjuster(Dictionary<string, double> Factors) : ISeasonalStatementOfWorkOverrideAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SeasonalStatementOfWorkOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSeasonalStatementOfWorkOverrideAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalStatementOfWorkOverrideAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalStatementOfWorkOverride item, double valueValue)
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

public sealed record RoundingSeasonalStatementOfWorkOverrideAdjuster(double Step) : ISeasonalStatementOfWorkOverrideAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalStatementOfWorkOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSeasonalStatementOfWorkOverrideAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalStatementOfWorkOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalStatementOfWorkOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal statement of work override steps in order and keeps a trace of each.</summary>
public sealed class SeasonalStatementOfWorkOverrideEngine(List<ISeasonalStatementOfWorkOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalStatementOfWorkOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalStatementOfWorkOverrideEngine DefaultSeasonalStatementOfWorkOverrideEngine(Registry flags)
    {
        return new SeasonalStatementOfWorkOverrideEngine(
            new List<ISeasonalStatementOfWorkOverrideAdjuster>
            {
                new TitleSeasonalStatementOfWorkOverrideAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.24, ["online"] = 1.22 }),
                new TieredSeasonalStatementOfWorkOverrideAdjuster(
                    new List<double> { 2352.0, 7031.0, 8432.0 },
                    new List<double> { 0.88, 0.94, 1.02 }),
                new RoundingSeasonalStatementOfWorkOverrideAdjuster(0.5),
                new FlaggedSeasonalStatementOfWorkOverrideAdjuster(
                    flags,
                    "seasonal-statement-of-work-override-weighted",
                    -8.3),
            },
            new List<string>(),
            3.3,
            90923.4);
    }

    public double Evaluate(SeasonalStatementOfWorkOverride item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalStatementOfWorkOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
