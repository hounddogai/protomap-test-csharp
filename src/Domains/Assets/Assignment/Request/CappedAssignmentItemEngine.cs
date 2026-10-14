namespace Acme.Workforce.Assets.Assignment.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped assignment item calculation.</summary>
public interface ICappedAssignmentItemAdjuster
{
    string Describe();
    double Adjust(CappedAssignmentItem item, double valueValue);
}

public sealed record MinimumCappedAssignmentItemAdjuster(double Minimum) : ICappedAssignmentItemAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedAssignmentItem item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CappedAssignmentItemScoreOffsetAdjuster(double Weight) : ICappedAssignmentItemAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(CappedAssignmentItem item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record DepartmentCappedAssignmentItemAdjuster(Dictionary<string, double> Factors) : ICappedAssignmentItemAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(CappedAssignmentItem item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCappedAssignmentItemAdjuster(double Cap) : ICappedAssignmentItemAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedAssignmentItem item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCappedAssignmentItemAdjuster(Registry Flags, string Flag, double Percent) : ICappedAssignmentItemAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedAssignmentItem item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCappedAssignmentItemAdjuster(double Percent) : ICappedAssignmentItemAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedAssignmentItem item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped assignment item steps in order and keeps a trace of each.</summary>
public sealed class CappedAssignmentItemEngine(List<ICappedAssignmentItemAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAssignmentItemAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAssignmentItemEngine DefaultCappedAssignmentItemEngine(Registry flags)
    {
        return new CappedAssignmentItemEngine(
            new List<ICappedAssignmentItemAdjuster>
            {
                new MinimumCappedAssignmentItemAdjuster(46.5),
                new CappedAssignmentItemScoreOffsetAdjuster(0.11),
                new DepartmentCappedAssignmentItemAdjuster(new Dictionary<string, double> { ["west"] = 1.11 }),
                new CapCappedAssignmentItemAdjuster(47679.0),
                new FlaggedCappedAssignmentItemAdjuster(flags, "capped-assignment-item-seasonal", 8.4),
                new PercentCappedAssignmentItemAdjuster(-5.6),
            },
            new List<string>(),
            1.8,
            18134.5);
    }

    public double Evaluate(CappedAssignmentItem item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedAssignmentItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
