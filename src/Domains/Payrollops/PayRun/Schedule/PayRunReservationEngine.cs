namespace Acme.Workforce.Payrollops.PayRun.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run reservation calculation.</summary>
public interface IPayRunReservationStep
{
    string Describe();
    double Adjust(PayRunReservation item, double valueValue);
}

public sealed record FlaggedPayRunReservationAdjuster(Registry Flags, string Flag, double Percent) : IPayRunReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPayRunReservationAdjuster(double Percent) : IPayRunReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionPayRunReservationAdjuster(Dictionary<string, double> Factors) : IPayRunReservationStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(PayRunReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pay run reservation steps in order and keeps a trace of each.</summary>
public sealed class PayRunReservationEngine(List<IPayRunReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunReservationEngine DefaultPayRunReservationEngine(Registry flags)
    {
        return new PayRunReservationEngine(
            new List<IPayRunReservationStep>
            {
                new FlaggedPayRunReservationAdjuster(flags, "pay-run-reservation-rolling", 9.5),
                new PercentPayRunReservationAdjuster(-19.7),
                new RegionPayRunReservationAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.09, ["east"] = 0.91 }),
            },
            new List<string>(),
            3.3,
            46380.6);
    }

    public double Evaluate(PayRunReservation item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
