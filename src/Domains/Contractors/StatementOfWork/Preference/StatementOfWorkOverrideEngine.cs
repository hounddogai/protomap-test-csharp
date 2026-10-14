namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work override calculation.</summary>
public interface IStatementOfWorkOverrideStep
{
    string Describe();
    double Adjust(StatementOfWorkOverride item, double valueValue);
}

public sealed record TieredStatementOfWorkOverrideAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkOverride item, double valueValue)
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

public sealed record FlaggedStatementOfWorkOverrideAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumStatementOfWorkOverrideAdjuster(double Minimum) : IStatementOfWorkOverrideStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record StatementOfWorkOverrideCreatedAtOffsetAdjuster(double Weight) : IStatementOfWorkOverrideStep
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(StatementOfWorkOverride item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record CountryStatementOfWorkOverrideAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkOverrideStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(StatementOfWorkOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the statement of work override steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkOverrideEngine(List<IStatementOfWorkOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkOverrideEngine DefaultStatementOfWorkOverrideEngine(Registry flags)
    {
        return new StatementOfWorkOverrideEngine(
            new List<IStatementOfWorkOverrideStep>
            {
                new TieredStatementOfWorkOverrideAdjuster(
                    new List<double> { 4605.0, 6727.0, 7607.0, 8202.0 },
                    new List<double> { 0.81, 1.17, 1.08, 1.02 }),
                new FlaggedStatementOfWorkOverrideAdjuster(flags, "statement-of-work-override-tiered", -6.1),
                new MinimumStatementOfWorkOverrideAdjuster(2.3),
                new StatementOfWorkOverrideCreatedAtOffsetAdjuster(1.5),
                new CountryStatementOfWorkOverrideAdjuster(new Dictionary<string, double> { ["east"] = 0.82 }),
            },
            new List<string>(),
            2.7,
            13036.1);
    }

    public double Evaluate(StatementOfWorkOverride item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
