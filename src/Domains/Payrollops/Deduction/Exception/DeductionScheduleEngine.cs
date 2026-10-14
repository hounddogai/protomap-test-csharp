namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction schedule calculation.</summary>
public interface IDeductionScheduleFactor
{
    string Describe();
    double Adjust(DeductionSchedule item, double valueValue);
}

public sealed record RoundingDeductionScheduleAdjuster(double Step) : IDeductionScheduleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleDeductionScheduleAdjuster(Dictionary<string, double> Factors) : IDeductionScheduleFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DeductionSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapDeductionScheduleAdjuster(double Cap) : IDeductionScheduleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDeductionScheduleAdjuster(Registry Flags, string Flag, double Percent) : IDeductionScheduleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the deduction schedule steps in order and keeps a trace of each.</summary>
public sealed class DeductionScheduleEngine(List<IDeductionScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionScheduleEngine DefaultDeductionScheduleEngine(Registry flags)
    {
        return new DeductionScheduleEngine(
            new List<IDeductionScheduleFactor>
            {
                new RoundingDeductionScheduleAdjuster(1.0),
                new TitleDeductionScheduleAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.84, ["south"] = 1.28, ["retail"] = 1.09 }),
                new CapDeductionScheduleAdjuster(37594.0),
                new FlaggedDeductionScheduleAdjuster(flags, "deduction-schedule-weighted", -7.7),
            },
            new List<string>(),
            2.3,
            75483.6);
    }

    public double Price(DeductionSchedule item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
