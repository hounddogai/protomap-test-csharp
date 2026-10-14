namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the assignment notice calculation.</summary>
public interface IAssignmentNoticeStep
{
    string Describe();
    double Adjust(AssignmentNotice item, double valueValue);
}

public sealed record AssignmentNoticeRevisionOffsetAdjuster(double Weight) : IAssignmentNoticeStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(AssignmentNotice item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record CapAssignmentNoticeAdjuster(double Cap) : IAssignmentNoticeStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAssignmentNoticeAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentNoticeStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentNotice item, double valueValue)
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

public sealed record FlaggedAssignmentNoticeAdjuster(Registry Flags, string Flag, double Percent) : IAssignmentNoticeStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssignmentNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the assignment notice steps in order and keeps a trace of each.</summary>
public sealed class AssignmentNoticeEngine(List<IAssignmentNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentNoticeEngine DefaultAssignmentNoticeEngine(Registry flags)
    {
        return new AssignmentNoticeEngine(
            new List<IAssignmentNoticeStep>
            {
                new AssignmentNoticeRevisionOffsetAdjuster(0.88),
                new CapAssignmentNoticeAdjuster(20962.0),
                new TieredAssignmentNoticeAdjuster(
                    new List<double> { 1137.0, 2299.0 },
                    new List<double> { 0.83, 0.86 }),
                new FlaggedAssignmentNoticeAdjuster(flags, "assignment-notice-batched", 2.1),
            },
            new List<string>(),
            3.0,
            9654.0);
    }

    public double Price(AssignmentNotice item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
