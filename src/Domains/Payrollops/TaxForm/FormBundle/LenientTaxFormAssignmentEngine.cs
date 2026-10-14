namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient tax form assignment calculation.</summary>
public interface ILenientTaxFormAssignmentModifier
{
    string Describe();
    double Adjust(LenientTaxFormAssignment item, double valueValue);
}

public sealed record RegionLenientTaxFormAssignmentAdjuster(Dictionary<string, double> Factors) : ILenientTaxFormAssignmentModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(LenientTaxFormAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLenientTaxFormAssignmentAdjuster(double Cap) : ILenientTaxFormAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientTaxFormAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LenientTaxFormAssignmentExpiresAtOffsetAdjuster(double Weight) : ILenientTaxFormAssignmentModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(LenientTaxFormAssignment item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record FlaggedLenientTaxFormAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ILenientTaxFormAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientTaxFormAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient tax form assignment steps in order and keeps a trace of each.</summary>
public sealed class LenientTaxFormAssignmentEngine(List<ILenientTaxFormAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientTaxFormAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientTaxFormAssignmentEngine DefaultLenientTaxFormAssignmentEngine(Registry flags)
    {
        return new LenientTaxFormAssignmentEngine(
            new List<ILenientTaxFormAssignmentModifier>
            {
                new RegionLenientTaxFormAssignmentAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.86, ["retail"] = 0.9, ["east"] = 0.8 }),
                new CapLenientTaxFormAssignmentAdjuster(46615.0),
                new LenientTaxFormAssignmentExpiresAtOffsetAdjuster(1.29),
                new FlaggedLenientTaxFormAssignmentAdjuster(flags, "lenient-tax-form-assignment-tiered", 1.9),
            },
            new List<string>(),
            4.7,
            38765.9);
    }

    public double Run(LenientTaxFormAssignment item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientTaxFormAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
