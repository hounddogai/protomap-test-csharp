namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet review calculation.</summary>
public interface ITimesheetReviewModifier
{
    string Describe();
    double Adjust(TimesheetReview item, double valueValue);
}

public sealed record RoundingTimesheetReviewAdjuster(double Step) : ITimesheetReviewModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTimesheetReviewAdjuster(double Minimum) : ITimesheetReviewModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTimesheetReviewAdjuster(double Cap) : ITimesheetReviewModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTimesheetReviewAdjuster(double Percent) : ITimesheetReviewModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TimesheetReviewRatioOffsetAdjuster(double Weight) : ITimesheetReviewModifier
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(TimesheetReview item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the timesheet review steps in order and keeps a trace of each.</summary>
public sealed class TimesheetReviewEngine(List<ITimesheetReviewModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetReviewModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetReviewEngine DefaultTimesheetReviewEngine()
    {
        return new TimesheetReviewEngine(
            new List<ITimesheetReviewModifier>
            {
                new RoundingTimesheetReviewAdjuster(0.25),
                new MinimumTimesheetReviewAdjuster(9.7),
                new CapTimesheetReviewAdjuster(33966.0),
                new PercentTimesheetReviewAdjuster(-1.6),
                new TimesheetReviewRatioOffsetAdjuster(0.91),
            },
            new List<string>(),
            2.3,
            46550.1);
    }

    public double Compute(TimesheetReview item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
