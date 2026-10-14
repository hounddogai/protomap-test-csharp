namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

/// <summary>One step of the capped offboarding schedule calculation.</summary>
public interface ICappedOffboardingScheduleFactor
{
    string Describe();
    double Adjust(CappedOffboardingSchedule item, double valueValue);
}

public sealed record CapCappedOffboardingScheduleAdjuster(double Cap) : ICappedOffboardingScheduleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedOffboardingSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumCappedOffboardingScheduleAdjuster(double Minimum) : ICappedOffboardingScheduleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedOffboardingSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentCappedOffboardingScheduleAdjuster(double Percent) : ICappedOffboardingScheduleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedOffboardingSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedOffboardingScheduleFloorValueOffsetAdjuster(double Weight) : ICappedOffboardingScheduleFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(CappedOffboardingSchedule item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

/// <summary>Runs the capped offboarding schedule steps in order and keeps a trace of each.</summary>
public sealed class CappedOffboardingScheduleEngine(List<ICappedOffboardingScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedOffboardingScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedOffboardingScheduleEngine DefaultCappedOffboardingScheduleEngine()
    {
        return new CappedOffboardingScheduleEngine(
            new List<ICappedOffboardingScheduleFactor>
            {
                new CapCappedOffboardingScheduleAdjuster(38208.0),
                new MinimumCappedOffboardingScheduleAdjuster(15.3),
                new PercentCappedOffboardingScheduleAdjuster(16.2),
                new CappedOffboardingScheduleFloorValueOffsetAdjuster(1.91),
            },
            new List<string>(),
            4.9,
            98998.7);
    }

    public double Price(CappedOffboardingSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedOffboardingSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
