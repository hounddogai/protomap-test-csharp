namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual tax form rollup calculation.</summary>
public interface IManualTaxFormRollupStep
{
    string Describe();
    double Adjust(ManualTaxFormRollup item, double valueValue);
}

public sealed record TieredManualTaxFormRollupAdjuster(List<double> Bounds, List<double> Rates) : IManualTaxFormRollupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualTaxFormRollup item, double valueValue)
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

public sealed record NameManualTaxFormRollupAdjuster(Dictionary<string, double> Factors) : IManualTaxFormRollupStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ManualTaxFormRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentManualTaxFormRollupAdjuster(double Percent) : IManualTaxFormRollupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualTaxFormRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualTaxFormRollupAdjuster(double Step) : IManualTaxFormRollupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualTaxFormRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedManualTaxFormRollupAdjuster(Registry Flags, string Flag, double Percent) : IManualTaxFormRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualTaxFormRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual tax form rollup steps in order and keeps a trace of each.</summary>
public sealed class ManualTaxFormRollupEngine(List<IManualTaxFormRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualTaxFormRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualTaxFormRollupEngine DefaultManualTaxFormRollupEngine(Registry flags)
    {
        return new ManualTaxFormRollupEngine(
            new List<IManualTaxFormRollupStep>
            {
                new TieredManualTaxFormRollupAdjuster(
                    new List<double> { 1970.0, 4593.0, 5822.0, 9334.0 },
                    new List<double> { 0.92, 1.08, 0.99, 0.91 }),
                new NameManualTaxFormRollupAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.76, ["north"] = 1.29, ["south"] = 1.19 }),
                new PercentManualTaxFormRollupAdjuster(-0.6),
                new RoundingManualTaxFormRollupAdjuster(5.0),
                new FlaggedManualTaxFormRollupAdjuster(flags, "manual-tax-form-rollup-global", -1.0),
            },
            new List<string>(),
            2.3,
            76800.0);
    }

    public double Evaluate(ManualTaxFormRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualTaxFormRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
