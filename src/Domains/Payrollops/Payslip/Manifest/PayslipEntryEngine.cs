namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the payslip entry calculation.</summary>
public interface IPayslipEntryModifier
{
    string Describe();
    double Adjust(PayslipEntry item, double valueValue);
}

public sealed record FlaggedPayslipEntryAdjuster(Registry Flags, string Flag, double Percent) : IPayslipEntryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayslipEntryAdjuster(List<double> Bounds, List<double> Rates) : IPayslipEntryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipEntry item, double valueValue)
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

public sealed record PayslipEntryScoreOffsetAdjuster(double Weight) : IPayslipEntryModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(PayslipEntry item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record PercentPayslipEntryAdjuster(double Percent) : IPayslipEntryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayslipEntryAdjuster(double Cap) : IPayslipEntryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitlePayslipEntryAdjuster(Dictionary<string, double> Factors) : IPayslipEntryModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PayslipEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the payslip entry steps in order and keeps a trace of each.</summary>
public sealed class PayslipEntryEngine(List<IPayslipEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipEntryEngine DefaultPayslipEntryEngine(Registry flags)
    {
        return new PayslipEntryEngine(
            new List<IPayslipEntryModifier>
            {
                new FlaggedPayslipEntryAdjuster(flags, "payslip-entry-manual", 1.5),
                new TieredPayslipEntryAdjuster(new List<double> { 2140.0, 5859.0 }, new List<double> { 0.92, 0.87 }),
                new PayslipEntryScoreOffsetAdjuster(0.44),
                new PercentPayslipEntryAdjuster(-24.5),
                new CapPayslipEntryAdjuster(42197.0),
                new TitlePayslipEntryAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.08, ["retail"] = 1.01, ["east"] = 1.07 }),
            },
            new List<string>(),
            4.7,
            96040.2);
    }

    public double Compute(PayslipEntry item)
    {
        var current = item.AdjustedTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<PayslipEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
