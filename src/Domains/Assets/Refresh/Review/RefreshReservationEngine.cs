namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh reservation calculation.</summary>
public interface IRefreshReservationStep
{
    string Describe();
    double Adjust(RefreshReservation item, double valueValue);
}

public sealed record CapRefreshReservationAdjuster(double Cap) : IRefreshReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRefreshReservationAdjuster(double Percent) : IRefreshReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RefreshReservationFteOffsetAdjuster(double Weight) : IRefreshReservationStep
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(RefreshReservation item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record MinimumRefreshReservationAdjuster(double Minimum) : IRefreshReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the refresh reservation steps in order and keeps a trace of each.</summary>
public sealed class RefreshReservationEngine(List<IRefreshReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshReservationEngine DefaultRefreshReservationEngine()
    {
        return new RefreshReservationEngine(
            new List<IRefreshReservationStep>
            {
                new CapRefreshReservationAdjuster(1522.0),
                new PercentRefreshReservationAdjuster(-8.7),
                new RefreshReservationFteOffsetAdjuster(0.14),
                new MinimumRefreshReservationAdjuster(24.5),
            },
            new List<string>(),
            4.7,
            43456.7);
    }

    public double Evaluate(RefreshReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<RefreshReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
