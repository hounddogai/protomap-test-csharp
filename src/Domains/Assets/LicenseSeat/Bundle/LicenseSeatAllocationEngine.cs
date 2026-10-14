namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat allocation calculation.</summary>
public interface ILicenseSeatAllocationFactor
{
    string Describe();
    double Adjust(LicenseSeatAllocation item, double valueValue);
}

public sealed record CapLicenseSeatAllocationAdjuster(double Cap) : ILicenseSeatAllocationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLicenseSeatAllocationAdjuster(double Minimum) : ILicenseSeatAllocationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LicenseSeatAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SourceLicenseSeatAllocationAdjuster(Dictionary<string, double> Factors) : ILicenseSeatAllocationFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LicenseSeatAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLicenseSeatAllocationAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLicenseSeatAllocationAdjuster(double Step) : ILicenseSeatAllocationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the license seat allocation steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatAllocationEngine(List<ILicenseSeatAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatAllocationEngine DefaultLicenseSeatAllocationEngine(Registry flags)
    {
        return new LicenseSeatAllocationEngine(
            new List<ILicenseSeatAllocationFactor>
            {
                new CapLicenseSeatAllocationAdjuster(19126.0),
                new MinimumLicenseSeatAllocationAdjuster(32.9),
                new SourceLicenseSeatAllocationAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.39, ["east"] = 0.79, ["north"] = 0.97 }),
                new FlaggedLicenseSeatAllocationAdjuster(flags, "license-seat-allocation-global", -3.1),
                new RoundingLicenseSeatAllocationAdjuster(0.05),
            },
            new List<string>(),
            0.0,
            5937.7);
    }

    public double Compute(LicenseSeatAllocation item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
