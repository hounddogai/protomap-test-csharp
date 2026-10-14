namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled pay run request calculation.</summary>
public interface IScheduledPayRunRequestModifier
{
    string Describe();
    double Adjust(ScheduledPayRunRequest item, double valueValue);
}

public sealed record CapScheduledPayRunRequestAdjuster(double Cap) : IScheduledPayRunRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledPayRunRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentScheduledPayRunRequestAdjuster(double Percent) : IScheduledPayRunRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledPayRunRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledPayRunRequestWindowDaysOffsetAdjuster(double Weight) : IScheduledPayRunRequestModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(ScheduledPayRunRequest item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record MinimumScheduledPayRunRequestAdjuster(double Minimum) : IScheduledPayRunRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledPayRunRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredScheduledPayRunRequestAdjuster(List<double> Bounds, List<double> Rates) : IScheduledPayRunRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledPayRunRequest item, double valueValue)
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

/// <summary>Runs the scheduled pay run request steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPayRunRequestEngine(List<IScheduledPayRunRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPayRunRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPayRunRequestEngine DefaultScheduledPayRunRequestEngine()
    {
        return new ScheduledPayRunRequestEngine(
            new List<IScheduledPayRunRequestModifier>
            {
                new CapScheduledPayRunRequestAdjuster(33498.0),
                new PercentScheduledPayRunRequestAdjuster(-1.7),
                new ScheduledPayRunRequestWindowDaysOffsetAdjuster(1.06),
                new MinimumScheduledPayRunRequestAdjuster(35.8),
                new TieredScheduledPayRunRequestAdjuster(
                    new List<double> { 1365.0, 5331.0, 9876.0 },
                    new List<double> { 0.93, 1.06, 1.03 }),
            },
            new List<string>(),
            4.6,
            74976.2);
    }

    public double Price(ScheduledPayRunRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPayRunRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
