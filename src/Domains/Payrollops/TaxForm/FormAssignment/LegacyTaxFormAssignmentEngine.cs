namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy tax form assignment calculation.</summary>
public interface ILegacyTaxFormAssignmentModifier
{
    string Describe();
    double Adjust(LegacyTaxFormAssignment item, double valueValue);
}

public sealed record FlaggedLegacyTaxFormAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTaxFormAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTaxFormAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LegacyTaxFormAssignmentExpiresAtOffsetAdjuster(double Weight) : ILegacyTaxFormAssignmentModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(LegacyTaxFormAssignment item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record CapLegacyTaxFormAssignmentAdjuster(double Cap) : ILegacyTaxFormAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyTaxFormAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLegacyTaxFormAssignmentAdjuster(double Percent) : ILegacyTaxFormAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyTaxFormAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyTaxFormAssignmentAdjuster(double Minimum) : ILegacyTaxFormAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyTaxFormAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy tax form assignment steps in order and keeps a trace of each.</summary>
public sealed class LegacyTaxFormAssignmentEngine(List<ILegacyTaxFormAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTaxFormAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTaxFormAssignmentEngine DefaultLegacyTaxFormAssignmentEngine(Registry flags)
    {
        return new LegacyTaxFormAssignmentEngine(
            new List<ILegacyTaxFormAssignmentModifier>
            {
                new FlaggedLegacyTaxFormAssignmentAdjuster(flags, "legacy-tax-form-assignment-rolling", -1.9),
                new LegacyTaxFormAssignmentExpiresAtOffsetAdjuster(0.91),
                new CapLegacyTaxFormAssignmentAdjuster(23965.0),
                new PercentLegacyTaxFormAssignmentAdjuster(17.3),
                new MinimumLegacyTaxFormAssignmentAdjuster(31.6),
            },
            new List<string>(),
            2.8,
            26003.0);
    }

    public double Run(LegacyTaxFormAssignment item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTaxFormAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
