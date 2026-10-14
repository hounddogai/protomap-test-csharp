namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending pay run agreement calculation.</summary>
public interface IPendingPayRunAgreementAdjuster
{
    string Describe();
    double Adjust(PendingPayRunAgreement item, double valueValue);
}

public sealed record CapPendingPayRunAgreementAdjuster(double Cap) : IPendingPayRunAgreementAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingPayRunAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPendingPayRunAgreementAdjuster(double Percent) : IPendingPayRunAgreementAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingPayRunAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PendingPayRunAgreementCostOffsetAdjuster(double Weight) : IPendingPayRunAgreementAdjuster
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(PendingPayRunAgreement item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record RoundingPendingPayRunAgreementAdjuster(double Step) : IPendingPayRunAgreementAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingPayRunAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending pay run agreement steps in order and keeps a trace of each.</summary>
public sealed class PendingPayRunAgreementEngine(List<IPendingPayRunAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingPayRunAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingPayRunAgreementEngine DefaultPendingPayRunAgreementEngine()
    {
        return new PendingPayRunAgreementEngine(
            new List<IPendingPayRunAgreementAdjuster>
            {
                new CapPendingPayRunAgreementAdjuster(29702.0),
                new PercentPendingPayRunAgreementAdjuster(12.0),
                new PendingPayRunAgreementCostOffsetAdjuster(0.14),
                new RoundingPendingPayRunAgreementAdjuster(0.05),
            },
            new List<string>(),
            0.5,
            84588.5);
    }

    public double Run(PendingPayRunAgreement item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PendingPayRunAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
