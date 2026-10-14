namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal payslip slot calculation.</summary>
public interface ISeasonalPayslipSlotStep
{
    string Describe();
    double Adjust(SeasonalPayslipSlot item, double valueValue);
}

public sealed record SeasonalPayslipSlotTenureMonthsOffsetAdjuster(double Weight) : ISeasonalPayslipSlotStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(SeasonalPayslipSlot item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record PercentSeasonalPayslipSlotAdjuster(double Percent) : ISeasonalPayslipSlotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalPayslipSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalPayslipSlotAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalPayslipSlotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalPayslipSlot item, double valueValue)
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

public sealed record TeamSeasonalPayslipSlotAdjuster(Dictionary<string, double> Factors) : ISeasonalPayslipSlotStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SeasonalPayslipSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the seasonal payslip slot steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPayslipSlotEngine(List<ISeasonalPayslipSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPayslipSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPayslipSlotEngine DefaultSeasonalPayslipSlotEngine()
    {
        return new SeasonalPayslipSlotEngine(
            new List<ISeasonalPayslipSlotStep>
            {
                new SeasonalPayslipSlotTenureMonthsOffsetAdjuster(0.9),
                new PercentSeasonalPayslipSlotAdjuster(13.3),
                new TieredSeasonalPayslipSlotAdjuster(
                    new List<double> { 2989.0, 7617.0, 8700.0 },
                    new List<double> { 1.06, 1.15, 1.01 }),
                new TeamSeasonalPayslipSlotAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.07, ["north"] = 1.38, ["central"] = 0.74 }),
            },
            new List<string>(),
            0.6,
            73079.2);
    }

    public double Run(SeasonalPayslipSlot item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPayslipSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
