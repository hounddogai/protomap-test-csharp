namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient succession plan record calculation.</summary>
public interface ILenientSuccessionPlanRecordFactor
{
    string Describe();
    double Adjust(LenientSuccessionPlanRecord item, double valueValue);
}

public sealed record LenientSuccessionPlanRecordSequenceOffsetAdjuster(double Weight) : ILenientSuccessionPlanRecordFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(LenientSuccessionPlanRecord item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record RoundingLenientSuccessionPlanRecordAdjuster(double Step) : ILenientSuccessionPlanRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientSuccessionPlanRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DepartmentLenientSuccessionPlanRecordAdjuster(Dictionary<string, double> Factors) : ILenientSuccessionPlanRecordFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(LenientSuccessionPlanRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLenientSuccessionPlanRecordAdjuster(Registry Flags, string Flag, double Percent) : ILenientSuccessionPlanRecordFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientSuccessionPlanRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient succession plan record steps in order and keeps a trace of each.</summary>
public sealed class LenientSuccessionPlanRecordEngine(List<ILenientSuccessionPlanRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientSuccessionPlanRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientSuccessionPlanRecordEngine DefaultLenientSuccessionPlanRecordEngine(Registry flags)
    {
        return new LenientSuccessionPlanRecordEngine(
            new List<ILenientSuccessionPlanRecordFactor>
            {
                new LenientSuccessionPlanRecordSequenceOffsetAdjuster(0.56),
                new RoundingLenientSuccessionPlanRecordAdjuster(0.25),
                new DepartmentLenientSuccessionPlanRecordAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.16, ["online"] = 0.92 }),
                new FlaggedLenientSuccessionPlanRecordAdjuster(flags, "lenient-succession-plan-record-capped", -1.0),
            },
            new List<string>(),
            2.6,
            19071.3);
    }

    public double Compute(LenientSuccessionPlanRecord item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientSuccessionPlanRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
