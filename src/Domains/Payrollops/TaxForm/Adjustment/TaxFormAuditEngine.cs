namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form audit calculation.</summary>
public interface ITaxFormAuditStep
{
    string Describe();
    double Adjust(TaxFormAudit item, double valueValue);
}

public sealed record MinimumTaxFormAuditAdjuster(double Minimum) : ITaxFormAuditStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTaxFormAuditAdjuster(double Cap) : ITaxFormAuditStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedTaxFormAuditAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormAuditStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTaxFormAuditAdjuster(double Step) : ITaxFormAuditStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tax form audit steps in order and keeps a trace of each.</summary>
public sealed class TaxFormAuditEngine(List<ITaxFormAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormAuditEngine DefaultTaxFormAuditEngine(Registry flags)
    {
        return new TaxFormAuditEngine(
            new List<ITaxFormAuditStep>
            {
                new MinimumTaxFormAuditAdjuster(40.1),
                new CapTaxFormAuditAdjuster(16106.0),
                new FlaggedTaxFormAuditAdjuster(flags, "tax-form-audit-scheduled", -9.1),
                new RoundingTaxFormAuditAdjuster(5.0),
            },
            new List<string>(),
            3.6,
            76717.1);
    }

    public double Evaluate(TaxFormAudit item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
