namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending tax form override calculation.</summary>
public interface IPendingTaxFormOverrideStep
{
    string Describe();
    double Adjust(PendingTaxFormOverride item, double valueValue);
}

public sealed record PercentPendingTaxFormOverrideAdjuster(double Percent) : IPendingTaxFormOverrideStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingTaxFormOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PendingTaxFormOverrideFloorValueOffsetAdjuster(double Weight) : IPendingTaxFormOverrideStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(PendingTaxFormOverride item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record TieredPendingTaxFormOverrideAdjuster(List<double> Bounds, List<double> Rates) : IPendingTaxFormOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingTaxFormOverride item, double valueValue)
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

public sealed record CapPendingTaxFormOverrideAdjuster(double Cap) : IPendingTaxFormOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingTaxFormOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPendingTaxFormOverrideAdjuster(Registry Flags, string Flag, double Percent) : IPendingTaxFormOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingTaxFormOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending tax form override steps in order and keeps a trace of each.</summary>
public sealed class PendingTaxFormOverrideEngine(List<IPendingTaxFormOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingTaxFormOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingTaxFormOverrideEngine DefaultPendingTaxFormOverrideEngine(Registry flags)
    {
        return new PendingTaxFormOverrideEngine(
            new List<IPendingTaxFormOverrideStep>
            {
                new PercentPendingTaxFormOverrideAdjuster(2.4),
                new PendingTaxFormOverrideFloorValueOffsetAdjuster(1.1),
                new TieredPendingTaxFormOverrideAdjuster(
                    new List<double> { 437.0, 2158.0, 8363.0 },
                    new List<double> { 1.08, 1.2, 1.01 }),
                new CapPendingTaxFormOverrideAdjuster(49872.0),
                new FlaggedPendingTaxFormOverrideAdjuster(flags, "pending-tax-form-override-lenient", -3.8),
            },
            new List<string>(),
            2.7,
            57449.1);
    }

    public double Run(PendingTaxFormOverride item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingTaxFormOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
