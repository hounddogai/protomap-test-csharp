namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy garnishment schedule calculation.</summary>
public interface ILegacyGarnishmentScheduleStep
{
    string Describe();
    double Adjust(LegacyGarnishmentSchedule item, double valueValue);
}

public sealed record MinimumLegacyGarnishmentScheduleAdjuster(double Minimum) : ILegacyGarnishmentScheduleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyGarnishmentSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLegacyGarnishmentScheduleAdjuster(double Cap) : ILegacyGarnishmentScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyGarnishmentSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLegacyGarnishmentScheduleAdjuster(double Percent) : ILegacyGarnishmentScheduleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyGarnishmentSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLegacyGarnishmentScheduleAdjuster(Registry Flags, string Flag, double Percent) : ILegacyGarnishmentScheduleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyGarnishmentSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy garnishment schedule steps in order and keeps a trace of each.</summary>
public sealed class LegacyGarnishmentScheduleEngine(List<ILegacyGarnishmentScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyGarnishmentScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyGarnishmentScheduleEngine DefaultLegacyGarnishmentScheduleEngine(Registry flags)
    {
        return new LegacyGarnishmentScheduleEngine(
            new List<ILegacyGarnishmentScheduleStep>
            {
                new MinimumLegacyGarnishmentScheduleAdjuster(23.4),
                new CapLegacyGarnishmentScheduleAdjuster(26294.0),
                new PercentLegacyGarnishmentScheduleAdjuster(-1.8),
                new FlaggedLegacyGarnishmentScheduleAdjuster(flags, "legacy-garnishment-schedule-legacy", 4.1),
            },
            new List<string>(),
            0.0,
            77829.7);
    }

    public double Price(LegacyGarnishmentSchedule item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyGarnishmentSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
