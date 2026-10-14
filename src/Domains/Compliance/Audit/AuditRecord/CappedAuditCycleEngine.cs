namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped audit cycle calculation.</summary>
public interface ICappedAuditCycleStep
{
    string Describe();
    double Adjust(CappedAuditCycle item, double valueValue);
}

public sealed record RoundingCappedAuditCycleAdjuster(double Step) : ICappedAuditCycleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedAuditCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CappedAuditCycleFteOffsetAdjuster(double Weight) : ICappedAuditCycleStep
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(CappedAuditCycle item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record MinimumCappedAuditCycleAdjuster(double Minimum) : ICappedAuditCycleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedAuditCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the capped audit cycle steps in order and keeps a trace of each.</summary>
public sealed class CappedAuditCycleEngine(List<ICappedAuditCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAuditCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAuditCycleEngine DefaultCappedAuditCycleEngine()
    {
        return new CappedAuditCycleEngine(
            new List<ICappedAuditCycleStep>
            {
                new RoundingCappedAuditCycleAdjuster(5.0),
                new CappedAuditCycleFteOffsetAdjuster(0.59),
                new MinimumCappedAuditCycleAdjuster(46.1),
            },
            new List<string>(),
            0.4,
            35188.3);
    }

    public double Run(CappedAuditCycle item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedAuditCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
