namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy leave request record calculation.</summary>
public interface ILegacyLeaveRequestRecordStep
{
    string Describe();
    double Adjust(LegacyLeaveRequestRecord item, double valueValue);
}

public sealed record PercentLegacyLeaveRequestRecordAdjuster(double Percent) : ILegacyLeaveRequestRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyLeaveRequestRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyLeaveRequestRecordAdjuster(double Minimum) : ILegacyLeaveRequestRecordStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyLeaveRequestRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLegacyLeaveRequestRecordAdjuster(double Step) : ILegacyLeaveRequestRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyLeaveRequestRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentLegacyLeaveRequestRecordAdjuster(Dictionary<string, double> Factors) : ILegacyLeaveRequestRecordStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(LegacyLeaveRequestRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the legacy leave request record steps in order and keeps a trace of each.</summary>
public sealed class LegacyLeaveRequestRecordEngine(List<ILegacyLeaveRequestRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyLeaveRequestRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyLeaveRequestRecordEngine DefaultLegacyLeaveRequestRecordEngine()
    {
        return new LegacyLeaveRequestRecordEngine(
            new List<ILegacyLeaveRequestRecordStep>
            {
                new PercentLegacyLeaveRequestRecordAdjuster(-16.0),
                new MinimumLegacyLeaveRequestRecordAdjuster(21.9),
                new RoundingLegacyLeaveRequestRecordAdjuster(0.5),
                new SegmentLegacyLeaveRequestRecordAdjuster(new Dictionary<string, double> { ["retail"] = 1.22 }),
            },
            new List<string>(),
            4.8,
            52113.4);
    }

    public double Evaluate(LegacyLeaveRequestRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyLeaveRequestRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
