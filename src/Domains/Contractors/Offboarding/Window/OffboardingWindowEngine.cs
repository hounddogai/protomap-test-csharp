namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding window calculation.</summary>
public interface IOffboardingWindowFactor
{
    string Describe();
    double Adjust(OffboardingWindow item, double valueValue);
}

public sealed record CapOffboardingWindowAdjuster(double Cap) : IOffboardingWindowFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record OffboardingWindowLeaveDaysOffsetAdjuster(double Weight) : IOffboardingWindowFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(OffboardingWindow item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record MinimumOffboardingWindowAdjuster(double Minimum) : IOffboardingWindowFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleOffboardingWindowAdjuster(Dictionary<string, double> Factors) : IOffboardingWindowFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(OffboardingWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredOffboardingWindowAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingWindowFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingWindow item, double valueValue)
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

/// <summary>Runs the offboarding window steps in order and keeps a trace of each.</summary>
public sealed class OffboardingWindowEngine(List<IOffboardingWindowFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingWindowFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingWindowEngine DefaultOffboardingWindowEngine()
    {
        return new OffboardingWindowEngine(
            new List<IOffboardingWindowFactor>
            {
                new CapOffboardingWindowAdjuster(30704.0),
                new OffboardingWindowLeaveDaysOffsetAdjuster(1.05),
                new MinimumOffboardingWindowAdjuster(36.1),
                new TitleOffboardingWindowAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.4, ["south"] = 1.39, ["east"] = 1.19 }),
                new TieredOffboardingWindowAdjuster(
                    new List<double> { 1610.0, 2343.0, 9347.0, 9599.0 },
                    new List<double> { 1.11, 0.99, 1.19, 0.88 }),
            },
            new List<string>(),
            2.8,
            1725.5);
    }

    public double Evaluate(OffboardingWindow item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
