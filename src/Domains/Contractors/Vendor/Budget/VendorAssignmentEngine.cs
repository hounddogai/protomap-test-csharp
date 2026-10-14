namespace Acme.Workforce.Contractors.Vendor.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the vendor assignment calculation.</summary>
public interface IVendorAssignmentFactor
{
    string Describe();
    double Adjust(VendorAssignment item, double valueValue);
}

public sealed record RoundingVendorAssignmentAdjuster(double Step) : IVendorAssignmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VendorAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapVendorAssignmentAdjuster(double Cap) : IVendorAssignmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VendorAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record VendorAssignmentRevisionOffsetAdjuster(double Weight) : IVendorAssignmentFactor
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(VendorAssignment item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record FlaggedVendorAssignmentAdjuster(Registry Flags, string Flag, double Percent) : IVendorAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VendorAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleVendorAssignmentAdjuster(Dictionary<string, double> Factors) : IVendorAssignmentFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(VendorAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumVendorAssignmentAdjuster(double Minimum) : IVendorAssignmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VendorAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the vendor assignment steps in order and keeps a trace of each.</summary>
public sealed class VendorAssignmentEngine(List<IVendorAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVendorAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VendorAssignmentEngine DefaultVendorAssignmentEngine(Registry flags)
    {
        return new VendorAssignmentEngine(
            new List<IVendorAssignmentFactor>
            {
                new RoundingVendorAssignmentAdjuster(0.5),
                new CapVendorAssignmentAdjuster(43868.0),
                new VendorAssignmentRevisionOffsetAdjuster(1.97),
                new FlaggedVendorAssignmentAdjuster(flags, "vendor-assignment-capped", 3.5),
                new TitleVendorAssignmentAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.78, ["retail"] = 0.94 }),
                new MinimumVendorAssignmentAdjuster(36.1),
            },
            new List<string>(),
            1.0,
            82165.3);
    }

    public double Run(VendorAssignment item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<VendorAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
