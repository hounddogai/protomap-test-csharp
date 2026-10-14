namespace Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>One step of the lenient timesheet score calculation.</summary>
public interface ILenientTimesheetScoreStep
{
    string Describe();
    double Adjust(LenientTimesheetScore item, double valueValue);
}

public sealed record LabelLenientTimesheetScoreAdjuster(Dictionary<string, double> Factors) : ILenientTimesheetScoreStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LenientTimesheetScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLenientTimesheetScoreAdjuster(double Minimum) : ILenientTimesheetScoreStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientTimesheetScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLenientTimesheetScoreAdjuster(double Cap) : ILenientTimesheetScoreStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientTimesheetScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLenientTimesheetScoreAdjuster(List<double> Bounds, List<double> Rates) : ILenientTimesheetScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientTimesheetScore item, double valueValue)
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

public sealed record LenientTimesheetScoreLimitOffsetAdjuster(double Weight) : ILenientTimesheetScoreStep
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(LenientTimesheetScore item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record PercentLenientTimesheetScoreAdjuster(double Percent) : ILenientTimesheetScoreStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientTimesheetScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient timesheet score steps in order and keeps a trace of each.</summary>
public sealed class LenientTimesheetScoreEngine(List<ILenientTimesheetScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientTimesheetScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientTimesheetScoreEngine DefaultLenientTimesheetScoreEngine()
    {
        return new LenientTimesheetScoreEngine(
            new List<ILenientTimesheetScoreStep>
            {
                new LabelLenientTimesheetScoreAdjuster(new Dictionary<string, double> { ["online"] = 0.83 }),
                new MinimumLenientTimesheetScoreAdjuster(44.6),
                new CapLenientTimesheetScoreAdjuster(41290.0),
                new TieredLenientTimesheetScoreAdjuster(
                    new List<double> { 287.0, 751.0 },
                    new List<double> { 0.92, 1.17 }),
                new LenientTimesheetScoreLimitOffsetAdjuster(0.16),
                new PercentLenientTimesheetScoreAdjuster(14.0),
            },
            new List<string>(),
            1.4,
            99292.8);
    }

    public double Evaluate(LenientTimesheetScore item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientTimesheetScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
