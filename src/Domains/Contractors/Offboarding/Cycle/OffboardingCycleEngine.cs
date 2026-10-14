namespace Acme.Workforce.Contractors.Offboarding.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding cycle calculation.</summary>
public interface IOffboardingCycleAdjuster
{
    string Describe();
    double Adjust(OffboardingCycle item, double valueValue);
}

public sealed record DepartmentOffboardingCycleAdjuster(Dictionary<string, double> Factors) : IOffboardingCycleAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(OffboardingCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedOffboardingCycleAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumOffboardingCycleAdjuster(double Minimum) : IOffboardingCycleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record OffboardingCycleAmountOffsetAdjuster(double Weight) : IOffboardingCycleAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(OffboardingCycle item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record PercentOffboardingCycleAdjuster(double Percent) : IOffboardingCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapOffboardingCycleAdjuster(double Cap) : IOffboardingCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the offboarding cycle steps in order and keeps a trace of each.</summary>
public sealed class OffboardingCycleEngine(List<IOffboardingCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingCycleEngine DefaultOffboardingCycleEngine(Registry flags)
    {
        return new OffboardingCycleEngine(
            new List<IOffboardingCycleAdjuster>
            {
                new DepartmentOffboardingCycleAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.98, ["central"] = 0.79, ["north"] = 1.28 }),
                new FlaggedOffboardingCycleAdjuster(flags, "offboarding-cycle-fallback", 0.5),
                new MinimumOffboardingCycleAdjuster(22.9),
                new OffboardingCycleAmountOffsetAdjuster(0.77),
                new PercentOffboardingCycleAdjuster(-14.0),
                new CapOffboardingCycleAdjuster(25161.0),
            },
            new List<string>(),
            4.9,
            65317.4);
    }

    public double Compute(OffboardingCycle item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
