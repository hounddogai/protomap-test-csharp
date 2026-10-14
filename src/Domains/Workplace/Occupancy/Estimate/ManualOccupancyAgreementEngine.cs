namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual occupancy agreement calculation.</summary>
public interface IManualOccupancyAgreementModifier
{
    string Describe();
    double Adjust(ManualOccupancyAgreement item, double valueValue);
}

public sealed record FlaggedManualOccupancyAgreementAdjuster(Registry Flags, string Flag, double Percent) : IManualOccupancyAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualOccupancyAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualOccupancyAgreementAdjuster(double Cap) : IManualOccupancyAgreementModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualOccupancyAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ManualOccupancyAgreementMinimumOffsetAdjuster(double Weight) : IManualOccupancyAgreementModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(ManualOccupancyAgreement item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the manual occupancy agreement steps in order and keeps a trace of each.</summary>
public sealed class ManualOccupancyAgreementEngine(List<IManualOccupancyAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualOccupancyAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualOccupancyAgreementEngine DefaultManualOccupancyAgreementEngine(Registry flags)
    {
        return new ManualOccupancyAgreementEngine(
            new List<IManualOccupancyAgreementModifier>
            {
                new FlaggedManualOccupancyAgreementAdjuster(flags, "manual-occupancy-agreement-lenient", -7.4),
                new CapManualOccupancyAgreementAdjuster(49620.0),
                new ManualOccupancyAgreementMinimumOffsetAdjuster(1.26),
            },
            new List<string>(),
            2.3,
            86122.9);
    }

    public double Evaluate(ManualOccupancyAgreement item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualOccupancyAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
