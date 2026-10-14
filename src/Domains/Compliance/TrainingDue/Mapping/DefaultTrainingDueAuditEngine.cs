namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default training due audit calculation.</summary>
public interface IDefaultTrainingDueAuditStep
{
    string Describe();
    double Adjust(DefaultTrainingDueAudit item, double valueValue);
}

public sealed record DefaultTrainingDueAuditUpdatedAtOffsetAdjuster(double Weight) : IDefaultTrainingDueAuditStep
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(DefaultTrainingDueAudit item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record FlaggedDefaultTrainingDueAuditAdjuster(Registry Flags, string Flag, double Percent) : IDefaultTrainingDueAuditStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultTrainingDueAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultTrainingDueAuditAdjuster(List<double> Bounds, List<double> Rates) : IDefaultTrainingDueAuditStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultTrainingDueAudit item, double valueValue)
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

public sealed record MinimumDefaultTrainingDueAuditAdjuster(double Minimum) : IDefaultTrainingDueAuditStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultTrainingDueAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultTrainingDueAuditAdjuster(double Percent) : IDefaultTrainingDueAuditStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultTrainingDueAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultTrainingDueAuditAdjuster(double Step) : IDefaultTrainingDueAuditStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultTrainingDueAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default training due audit steps in order and keeps a trace of each.</summary>
public sealed class DefaultTrainingDueAuditEngine(List<IDefaultTrainingDueAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultTrainingDueAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultTrainingDueAuditEngine DefaultDefaultTrainingDueAuditEngine(Registry flags)
    {
        return new DefaultTrainingDueAuditEngine(
            new List<IDefaultTrainingDueAuditStep>
            {
                new DefaultTrainingDueAuditUpdatedAtOffsetAdjuster(0.87),
                new FlaggedDefaultTrainingDueAuditAdjuster(flags, "default-training-due-audit-regional", -9.4),
                new TieredDefaultTrainingDueAuditAdjuster(
                    new List<double> { 2086.0, 2473.0, 5699.0 },
                    new List<double> { 0.81, 1.13, 0.87 }),
                new MinimumDefaultTrainingDueAuditAdjuster(19.0),
                new PercentDefaultTrainingDueAuditAdjuster(-9.8),
                new RoundingDefaultTrainingDueAuditAdjuster(0.25),
            },
            new List<string>(),
            1.2,
            88987.2);
    }

    public double Price(DefaultTrainingDueAudit item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultTrainingDueAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
