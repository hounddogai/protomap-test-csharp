namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual quota calculation.</summary>
public interface IAccrualQuotaAdjuster
{
    string Describe();
    double Adjust(AccrualQuota item, double valueValue);
}

public sealed record FlaggedAccrualQuotaAdjuster(Registry Flags, string Flag, double Percent) : IAccrualQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccrualQuotaAdjuster(double Step) : IAccrualQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAccrualQuotaAdjuster(double Cap) : IAccrualQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AccrualQuotaVersionOffsetAdjuster(double Weight) : IAccrualQuotaAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(AccrualQuota item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record PercentAccrualQuotaAdjuster(double Percent) : IAccrualQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAccrualQuotaAdjuster(double Minimum) : IAccrualQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the accrual quota steps in order and keeps a trace of each.</summary>
public sealed class AccrualQuotaEngine(List<IAccrualQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualQuotaEngine DefaultAccrualQuotaEngine(Registry flags)
    {
        return new AccrualQuotaEngine(
            new List<IAccrualQuotaAdjuster>
            {
                new FlaggedAccrualQuotaAdjuster(flags, "accrual-quota-manual", 8.6),
                new RoundingAccrualQuotaAdjuster(0.1),
                new CapAccrualQuotaAdjuster(1088.0),
                new AccrualQuotaVersionOffsetAdjuster(0.09),
                new PercentAccrualQuotaAdjuster(-18.3),
                new MinimumAccrualQuotaAdjuster(15.0),
            },
            new List<string>(),
            1.8,
            87545.0);
    }

    public double Price(AccrualQuota item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
