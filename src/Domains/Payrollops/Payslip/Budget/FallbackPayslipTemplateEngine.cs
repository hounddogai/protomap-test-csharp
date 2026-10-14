namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback payslip template calculation.</summary>
public interface IFallbackPayslipTemplateFactor
{
    string Describe();
    double Adjust(FallbackPayslipTemplate item, double valueValue);
}

public sealed record FallbackPayslipTemplateSequenceOffsetAdjuster(double Weight) : IFallbackPayslipTemplateFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(FallbackPayslipTemplate item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record TieredFallbackPayslipTemplateAdjuster(List<double> Bounds, List<double> Rates) : IFallbackPayslipTemplateFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackPayslipTemplate item, double valueValue)
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

public sealed record PercentFallbackPayslipTemplateAdjuster(double Percent) : IFallbackPayslipTemplateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackPayslipTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackPayslipTemplateAdjuster(double Minimum) : IFallbackPayslipTemplateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPayslipTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFallbackPayslipTemplateAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPayslipTemplateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPayslipTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackPayslipTemplateAdjuster(double Step) : IFallbackPayslipTemplateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackPayslipTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback payslip template steps in order and keeps a trace of each.</summary>
public sealed class FallbackPayslipTemplateEngine(List<IFallbackPayslipTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPayslipTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPayslipTemplateEngine DefaultFallbackPayslipTemplateEngine(Registry flags)
    {
        return new FallbackPayslipTemplateEngine(
            new List<IFallbackPayslipTemplateFactor>
            {
                new FallbackPayslipTemplateSequenceOffsetAdjuster(1.35),
                new TieredFallbackPayslipTemplateAdjuster(
                    new List<double> { 5779.0, 7472.0, 9792.0 },
                    new List<double> { 0.96, 0.95, 0.9 }),
                new PercentFallbackPayslipTemplateAdjuster(11.5),
                new MinimumFallbackPayslipTemplateAdjuster(22.1),
                new FlaggedFallbackPayslipTemplateAdjuster(flags, "fallback-payslip-template-global", 7.5),
                new RoundingFallbackPayslipTemplateAdjuster(0.25),
            },
            new List<string>(),
            4.5,
            12980.0);
    }

    public double Compute(FallbackPayslipTemplate item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPayslipTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
