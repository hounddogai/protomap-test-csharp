namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global license seat assignment calculation.</summary>
public interface IGlobalLicenseSeatAssignmentStep
{
    string Describe();
    double Adjust(GlobalLicenseSeatAssignment item, double valueValue);
}

public sealed record PercentGlobalLicenseSeatAssignmentAdjuster(double Percent) : IGlobalLicenseSeatAssignmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalLicenseSeatAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedGlobalLicenseSeatAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IGlobalLicenseSeatAssignmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalLicenseSeatAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredGlobalLicenseSeatAssignmentAdjuster(List<double> Bounds, List<double> Rates) : IGlobalLicenseSeatAssignmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalLicenseSeatAssignment item, double valueValue)
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

public sealed record MinimumGlobalLicenseSeatAssignmentAdjuster(double Minimum) : IGlobalLicenseSeatAssignmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalLicenseSeatAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapGlobalLicenseSeatAssignmentAdjuster(double Cap) : IGlobalLicenseSeatAssignmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalLicenseSeatAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the global license seat assignment steps in order and keeps a trace of each.</summary>
public sealed class GlobalLicenseSeatAssignmentEngine(List<IGlobalLicenseSeatAssignmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalLicenseSeatAssignmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalLicenseSeatAssignmentEngine DefaultGlobalLicenseSeatAssignmentEngine(Registry flags)
    {
        return new GlobalLicenseSeatAssignmentEngine(
            new List<IGlobalLicenseSeatAssignmentStep>
            {
                new PercentGlobalLicenseSeatAssignmentAdjuster(-1.1),
                new FlaggedGlobalLicenseSeatAssignmentAdjuster(flags, "global-license-seat-assignment-rolling", 0.4),
                new TieredGlobalLicenseSeatAssignmentAdjuster(
                    new List<double> { 2233.0, 2503.0, 5419.0 },
                    new List<double> { 0.98, 0.83, 1.1 }),
                new MinimumGlobalLicenseSeatAssignmentAdjuster(35.5),
                new CapGlobalLicenseSeatAssignmentAdjuster(11077.0),
            },
            new List<string>(),
            1.2,
            78616.5);
    }

    public double Run(GlobalLicenseSeatAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalLicenseSeatAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
