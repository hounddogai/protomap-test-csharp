namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled payslip template calculation.</summary>
public interface IScheduledPayslipTemplateAdjuster
{
    string Describe();
    double Adjust(ScheduledPayslipTemplate item, double valueValue);
}

public sealed record TieredScheduledPayslipTemplateAdjuster(List<double> Bounds, List<double> Rates) : IScheduledPayslipTemplateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledPayslipTemplate item, double valueValue)
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

public sealed record RoundingScheduledPayslipTemplateAdjuster(double Step) : IScheduledPayslipTemplateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledPayslipTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapScheduledPayslipTemplateAdjuster(double Cap) : IScheduledPayslipTemplateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledPayslipTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled payslip template steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPayslipTemplateEngine(List<IScheduledPayslipTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPayslipTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPayslipTemplateEngine DefaultScheduledPayslipTemplateEngine()
    {
        return new ScheduledPayslipTemplateEngine(
            new List<IScheduledPayslipTemplateAdjuster>
            {
                new TieredScheduledPayslipTemplateAdjuster(
                    new List<double> { 7148.0, 7664.0, 9115.0 },
                    new List<double> { 0.87, 1.04, 1.11 }),
                new RoundingScheduledPayslipTemplateAdjuster(0.1),
                new CapScheduledPayslipTemplateAdjuster(45468.0),
            },
            new List<string>(),
            3.4,
            47317.5);
    }

    public double Evaluate(ScheduledPayslipTemplate item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPayslipTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
