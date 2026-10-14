namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work schedule calculation.</summary>
public interface IStatementOfWorkScheduleAdjuster
{
    string Describe();
    double Adjust(StatementOfWorkSchedule item, double valueValue);
}

public sealed record PercentStatementOfWorkScheduleAdjuster(double Percent) : IStatementOfWorkScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StatementOfWorkSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StatementOfWorkScheduleCreatedAtOffsetAdjuster(double Weight) : IStatementOfWorkScheduleAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(StatementOfWorkSchedule item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record RoundingStatementOfWorkScheduleAdjuster(double Step) : IStatementOfWorkScheduleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredStatementOfWorkScheduleAdjuster(List<double> Bounds, List<double> Rates) : IStatementOfWorkScheduleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StatementOfWorkSchedule item, double valueValue)
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

public sealed record FlaggedStatementOfWorkScheduleAdjuster(Registry Flags, string Flag, double Percent) : IStatementOfWorkScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StatementOfWorkSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameStatementOfWorkScheduleAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkScheduleAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(StatementOfWorkSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the statement of work schedule steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkScheduleEngine(List<IStatementOfWorkScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkScheduleEngine DefaultStatementOfWorkScheduleEngine(Registry flags)
    {
        return new StatementOfWorkScheduleEngine(
            new List<IStatementOfWorkScheduleAdjuster>
            {
                new PercentStatementOfWorkScheduleAdjuster(23.6),
                new StatementOfWorkScheduleCreatedAtOffsetAdjuster(0.1),
                new RoundingStatementOfWorkScheduleAdjuster(0.1),
                new TieredStatementOfWorkScheduleAdjuster(
                    new List<double> { 737.0, 1413.0 },
                    new List<double> { 1.07, 1.13 }),
                new FlaggedStatementOfWorkScheduleAdjuster(flags, "statement-of-work-schedule-seasonal", -1.6),
                new NameStatementOfWorkScheduleAdjuster(new Dictionary<string, double> { ["central"] = 1.2 }),
            },
            new List<string>(),
            0.4,
            62117.8);
    }

    public double Run(StatementOfWorkSchedule item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
