namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip template calculation.</summary>
public interface IPayslipTemplateModifier
{
    string Describe();
    double Adjust(PayslipTemplate item, double valueValue);
}

public sealed record MinimumPayslipTemplateAdjuster(double Minimum) : IPayslipTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPayslipTemplateAdjuster(double Cap) : IPayslipTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitlePayslipTemplateAdjuster(Dictionary<string, double> Factors) : IPayslipTemplateModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PayslipTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPayslipTemplateAdjuster(List<double> Bounds, List<double> Rates) : IPayslipTemplateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipTemplate item, double valueValue)
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

public sealed record FlaggedPayslipTemplateAdjuster(Registry Flags, string Flag, double Percent) : IPayslipTemplateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the payslip template steps in order and keeps a trace of each.</summary>
public sealed class PayslipTemplateEngine(List<IPayslipTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipTemplateEngine DefaultPayslipTemplateEngine(Registry flags)
    {
        return new PayslipTemplateEngine(
            new List<IPayslipTemplateModifier>
            {
                new MinimumPayslipTemplateAdjuster(7.2),
                new CapPayslipTemplateAdjuster(28597.0),
                new TitlePayslipTemplateAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.7, ["south"] = 1.3, ["west"] = 1.07 }),
                new TieredPayslipTemplateAdjuster(
                    new List<double> { 419.0, 3722.0, 8239.0 },
                    new List<double> { 0.96, 0.85, 0.9 }),
                new FlaggedPayslipTemplateAdjuster(flags, "payslip-template-batched", -7.7),
            },
            new List<string>(),
            3.8,
            47559.8);
    }

    public double Compute(PayslipTemplate item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
