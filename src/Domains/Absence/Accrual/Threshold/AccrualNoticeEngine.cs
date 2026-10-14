namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual notice calculation.</summary>
public interface IAccrualNoticeStep
{
    string Describe();
    double Adjust(AccrualNotice item, double valueValue);
}

public sealed record CapAccrualNoticeAdjuster(double Cap) : IAccrualNoticeStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CategoryAccrualNoticeAdjuster(Dictionary<string, double> Factors) : IAccrualNoticeStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(AccrualNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAccrualNoticeAdjuster(double Step) : IAccrualNoticeStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the accrual notice steps in order and keeps a trace of each.</summary>
public sealed class AccrualNoticeEngine(List<IAccrualNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualNoticeEngine DefaultAccrualNoticeEngine()
    {
        return new AccrualNoticeEngine(
            new List<IAccrualNoticeStep>
            {
                new CapAccrualNoticeAdjuster(39545.0),
                new CategoryAccrualNoticeAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.25, ["central"] = 1.11 }),
                new RoundingAccrualNoticeAdjuster(1.0),
            },
            new List<string>(),
            1.3,
            94637.7);
    }

    public double Compute(AccrualNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<AccrualNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
