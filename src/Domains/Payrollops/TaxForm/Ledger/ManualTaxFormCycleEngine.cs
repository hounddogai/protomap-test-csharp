namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual tax form cycle calculation.</summary>
public interface IManualTaxFormCycleAdjuster
{
    string Describe();
    double Adjust(ManualTaxFormCycle item, double valueValue);
}

public sealed record LocaleManualTaxFormCycleAdjuster(Dictionary<string, double> Factors) : IManualTaxFormCycleAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(ManualTaxFormCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumManualTaxFormCycleAdjuster(double Minimum) : IManualTaxFormCycleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualTaxFormCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapManualTaxFormCycleAdjuster(double Cap) : IManualTaxFormCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualTaxFormCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual tax form cycle steps in order and keeps a trace of each.</summary>
public sealed class ManualTaxFormCycleEngine(List<IManualTaxFormCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualTaxFormCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualTaxFormCycleEngine DefaultManualTaxFormCycleEngine()
    {
        return new ManualTaxFormCycleEngine(
            new List<IManualTaxFormCycleAdjuster>
            {
                new LocaleManualTaxFormCycleAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.73, ["online"] = 1.08 }),
                new MinimumManualTaxFormCycleAdjuster(30.6),
                new CapManualTaxFormCycleAdjuster(836.0),
            },
            new List<string>(),
            2.4,
            68091.7);
    }

    public double Evaluate(ManualTaxFormCycle item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualTaxFormCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
