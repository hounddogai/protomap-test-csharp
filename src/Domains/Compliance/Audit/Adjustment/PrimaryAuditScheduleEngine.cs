namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary audit schedule calculation.</summary>
public interface IPrimaryAuditScheduleModifier
{
    string Describe();
    double Adjust(PrimaryAuditSchedule item, double valueValue);
}

public sealed record NamePrimaryAuditScheduleAdjuster(Dictionary<string, double> Factors) : IPrimaryAuditScheduleModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PrimaryAuditSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPrimaryAuditScheduleAdjuster(double Step) : IPrimaryAuditScheduleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryAuditSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPrimaryAuditScheduleAdjuster(double Cap) : IPrimaryAuditScheduleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryAuditSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPrimaryAuditScheduleAdjuster(double Minimum) : IPrimaryAuditScheduleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryAuditSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary audit schedule steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAuditScheduleEngine(List<IPrimaryAuditScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAuditScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAuditScheduleEngine DefaultPrimaryAuditScheduleEngine()
    {
        return new PrimaryAuditScheduleEngine(
            new List<IPrimaryAuditScheduleModifier>
            {
                new NamePrimaryAuditScheduleAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.85, ["central"] = 1.19, ["north"] = 1.23 }),
                new RoundingPrimaryAuditScheduleAdjuster(0.25),
                new CapPrimaryAuditScheduleAdjuster(4040.0),
                new MinimumPrimaryAuditScheduleAdjuster(29.7),
            },
            new List<string>(),
            0.7,
            37737.7);
    }

    public double Compute(PrimaryAuditSchedule item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAuditSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
