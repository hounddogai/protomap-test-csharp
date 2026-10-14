namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global payslip checkpoint calculation.</summary>
public interface IGlobalPayslipCheckpointStep
{
    string Describe();
    double Adjust(GlobalPayslipCheckpoint item, double valueValue);
}

public sealed record RoundingGlobalPayslipCheckpointAdjuster(double Step) : IGlobalPayslipCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalPayslipCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentGlobalPayslipCheckpointAdjuster(double Percent) : IGlobalPayslipCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalPayslipCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredGlobalPayslipCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IGlobalPayslipCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalPayslipCheckpoint item, double valueValue)
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

public sealed record MinimumGlobalPayslipCheckpointAdjuster(double Minimum) : IGlobalPayslipCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalPayslipCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the global payslip checkpoint steps in order and keeps a trace of each.</summary>
public sealed class GlobalPayslipCheckpointEngine(List<IGlobalPayslipCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPayslipCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPayslipCheckpointEngine DefaultGlobalPayslipCheckpointEngine()
    {
        return new GlobalPayslipCheckpointEngine(
            new List<IGlobalPayslipCheckpointStep>
            {
                new RoundingGlobalPayslipCheckpointAdjuster(1.0),
                new PercentGlobalPayslipCheckpointAdjuster(9.7),
                new TieredGlobalPayslipCheckpointAdjuster(
                    new List<double> { 1211.0, 4902.0, 6994.0, 8179.0 },
                    new List<double> { 1.09, 0.94, 1.16, 0.89 }),
                new MinimumGlobalPayslipCheckpointAdjuster(14.6),
            },
            new List<string>(),
            3.6,
            21648.7);
    }

    public double Evaluate(GlobalPayslipCheckpoint item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPayslipCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
