namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient leave request reservation calculation.</summary>
public interface ILenientLeaveRequestReservationStep
{
    string Describe();
    double Adjust(LenientLeaveRequestReservation item, double valueValue);
}

public sealed record LenientLeaveRequestReservationBudgetOffsetAdjuster(double Weight) : ILenientLeaveRequestReservationStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(LenientLeaveRequestReservation item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record PercentLenientLeaveRequestReservationAdjuster(double Percent) : ILenientLeaveRequestReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientLeaveRequestReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientLeaveRequestReservationAdjuster(double Step) : ILenientLeaveRequestReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientLeaveRequestReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleLenientLeaveRequestReservationAdjuster(Dictionary<string, double> Factors) : ILenientLeaveRequestReservationStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LenientLeaveRequestReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient leave request reservation steps in order and keeps a trace of each.</summary>
public sealed class LenientLeaveRequestReservationEngine(List<ILenientLeaveRequestReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientLeaveRequestReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientLeaveRequestReservationEngine DefaultLenientLeaveRequestReservationEngine()
    {
        return new LenientLeaveRequestReservationEngine(
            new List<ILenientLeaveRequestReservationStep>
            {
                new LenientLeaveRequestReservationBudgetOffsetAdjuster(0.47),
                new PercentLenientLeaveRequestReservationAdjuster(11.9),
                new RoundingLenientLeaveRequestReservationAdjuster(0.25),
                new TitleLenientLeaveRequestReservationAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.79, ["west"] = 1.0 }),
            },
            new List<string>(),
            3.0,
            33212.6);
    }

    public double Price(LenientLeaveRequestReservation item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientLeaveRequestReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
