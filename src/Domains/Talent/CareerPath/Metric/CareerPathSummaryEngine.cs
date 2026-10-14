namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the career path summary calculation.</summary>
public interface ICareerPathSummaryAdjuster
{
    string Describe();
    double Adjust(CareerPathSummary item, double valueValue);
}

public sealed record CapCareerPathSummaryAdjuster(double Cap) : ICareerPathSummaryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentCareerPathSummaryAdjuster(double Percent) : ICareerPathSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCareerPathSummaryAdjuster(double Minimum) : ICareerPathSummaryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCareerPathSummaryAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCareerPathSummaryAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathSummaryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathSummary item, double valueValue)
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

public sealed record CareerPathSummaryEndsAtOffsetAdjuster(double Weight) : ICareerPathSummaryAdjuster
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(CareerPathSummary item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the career path summary steps in order and keeps a trace of each.</summary>
public sealed class CareerPathSummaryEngine(List<ICareerPathSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathSummaryEngine DefaultCareerPathSummaryEngine(Registry flags)
    {
        return new CareerPathSummaryEngine(
            new List<ICareerPathSummaryAdjuster>
            {
                new CapCareerPathSummaryAdjuster(49523.0),
                new PercentCareerPathSummaryAdjuster(-20.4),
                new MinimumCareerPathSummaryAdjuster(47.0),
                new FlaggedCareerPathSummaryAdjuster(flags, "career-path-summary-rolling", -5.6),
                new TieredCareerPathSummaryAdjuster(
                    new List<double> { 1032.0, 5470.0 },
                    new List<double> { 0.87, 0.94 }),
                new CareerPathSummaryEndsAtOffsetAdjuster(0.04),
            },
            new List<string>(),
            4.4,
            76094.2);
    }

    public double Evaluate(CareerPathSummary item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
