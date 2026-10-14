namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding schedule calculation.</summary>
public interface IOffboardingScheduleAdjuster
{
    string Describe();
    double Adjust(OffboardingSchedule item, double valueValue);
}

public sealed record CapOffboardingScheduleAdjuster(double Cap) : IOffboardingScheduleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumOffboardingScheduleAdjuster(double Minimum) : IOffboardingScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredOffboardingScheduleAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingScheduleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingSchedule item, double valueValue)
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

public sealed record PercentOffboardingScheduleAdjuster(double Percent) : IOffboardingScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the offboarding schedule steps in order and keeps a trace of each.</summary>
public sealed class OffboardingScheduleEngine(List<IOffboardingScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingScheduleEngine DefaultOffboardingScheduleEngine()
    {
        return new OffboardingScheduleEngine(
            new List<IOffboardingScheduleAdjuster>
            {
                new CapOffboardingScheduleAdjuster(2232.0),
                new MinimumOffboardingScheduleAdjuster(25.1),
                new TieredOffboardingScheduleAdjuster(
                    new List<double> { 1988.0, 4063.0, 5169.0, 7551.0 },
                    new List<double> { 1.16, 1.09, 0.87, 1.03 }),
                new PercentOffboardingScheduleAdjuster(-9.3),
            },
            new List<string>(),
            3.1,
            30534.0);
    }

    public double Price(OffboardingSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
