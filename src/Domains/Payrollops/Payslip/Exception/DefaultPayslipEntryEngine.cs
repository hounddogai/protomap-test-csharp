namespace Acme.Workforce.Payrollops.Payslip.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default payslip entry calculation.</summary>
public interface IDefaultPayslipEntryFactor
{
    string Describe();
    double Adjust(DefaultPayslipEntry item, double valueValue);
}

public sealed record RoundingDefaultPayslipEntryAdjuster(double Step) : IDefaultPayslipEntryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPayslipEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultPayslipEntryAdjuster(double Percent) : IDefaultPayslipEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPayslipEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultPayslipEntryAdjuster(double Minimum) : IDefaultPayslipEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPayslipEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapDefaultPayslipEntryAdjuster(double Cap) : IDefaultPayslipEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultPayslipEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourceDefaultPayslipEntryAdjuster(Dictionary<string, double> Factors) : IDefaultPayslipEntryFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(DefaultPayslipEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the default payslip entry steps in order and keeps a trace of each.</summary>
public sealed class DefaultPayslipEntryEngine(List<IDefaultPayslipEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPayslipEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPayslipEntryEngine DefaultDefaultPayslipEntryEngine()
    {
        return new DefaultPayslipEntryEngine(
            new List<IDefaultPayslipEntryFactor>
            {
                new RoundingDefaultPayslipEntryAdjuster(0.25),
                new PercentDefaultPayslipEntryAdjuster(-23.5),
                new MinimumDefaultPayslipEntryAdjuster(46.0),
                new CapDefaultPayslipEntryAdjuster(765.0),
                new SourceDefaultPayslipEntryAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.09, ["south"] = 0.98 }),
            },
            new List<string>(),
            4.3,
            77368.2);
    }

    public double Price(DefaultPayslipEntry item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPayslipEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
