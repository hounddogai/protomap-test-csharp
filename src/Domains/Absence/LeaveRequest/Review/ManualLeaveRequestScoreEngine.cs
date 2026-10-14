namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the manual leave request score calculation.</summary>
public interface IManualLeaveRequestScoreStep
{
    string Describe();
    double Adjust(ManualLeaveRequestScore item, double valueValue);
}

public sealed record CategoryManualLeaveRequestScoreAdjuster(Dictionary<string, double> Factors) : IManualLeaveRequestScoreStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(ManualLeaveRequestScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumManualLeaveRequestScoreAdjuster(double Minimum) : IManualLeaveRequestScoreStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualLeaveRequestScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapManualLeaveRequestScoreAdjuster(double Cap) : IManualLeaveRequestScoreStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualLeaveRequestScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ManualLeaveRequestScoreWindowDaysOffsetAdjuster(double Weight) : IManualLeaveRequestScoreStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(ManualLeaveRequestScore item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record TieredManualLeaveRequestScoreAdjuster(List<double> Bounds, List<double> Rates) : IManualLeaveRequestScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualLeaveRequestScore item, double valueValue)
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

public sealed record FlaggedManualLeaveRequestScoreAdjuster(Registry Flags, string Flag, double Percent) : IManualLeaveRequestScoreStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualLeaveRequestScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual leave request score steps in order and keeps a trace of each.</summary>
public sealed class ManualLeaveRequestScoreEngine(List<IManualLeaveRequestScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualLeaveRequestScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualLeaveRequestScoreEngine DefaultManualLeaveRequestScoreEngine(Registry flags)
    {
        return new ManualLeaveRequestScoreEngine(
            new List<IManualLeaveRequestScoreStep>
            {
                new CategoryManualLeaveRequestScoreAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.04, ["west"] = 1.12, ["online"] = 0.78 }),
                new MinimumManualLeaveRequestScoreAdjuster(45.8),
                new CapManualLeaveRequestScoreAdjuster(38057.0),
                new ManualLeaveRequestScoreWindowDaysOffsetAdjuster(1.83),
                new TieredManualLeaveRequestScoreAdjuster(
                    new List<double> { 293.0, 7956.0, 8503.0, 9053.0 },
                    new List<double> { 1.1, 0.91, 1.07, 1.0 }),
                new FlaggedManualLeaveRequestScoreAdjuster(flags, "manual-leave-request-score-primary", 3.6),
            },
            new List<string>(),
            1.3,
            58204.1);
    }

    public double Evaluate(ManualLeaveRequestScore item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualLeaveRequestScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
