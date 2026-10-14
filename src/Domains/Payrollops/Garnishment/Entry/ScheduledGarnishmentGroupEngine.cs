namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled garnishment group calculation.</summary>
public interface IScheduledGarnishmentGroupStep
{
    string Describe();
    double Adjust(ScheduledGarnishmentGroup item, double valueValue);
}

public sealed record CapScheduledGarnishmentGroupAdjuster(double Cap) : IScheduledGarnishmentGroupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledGarnishmentGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ScheduledGarnishmentGroupMaximumOffsetAdjuster(double Weight) : IScheduledGarnishmentGroupStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(ScheduledGarnishmentGroup item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record MinimumScheduledGarnishmentGroupAdjuster(double Minimum) : IScheduledGarnishmentGroupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledGarnishmentGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled garnishment group steps in order and keeps a trace of each.</summary>
public sealed class ScheduledGarnishmentGroupEngine(List<IScheduledGarnishmentGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledGarnishmentGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledGarnishmentGroupEngine DefaultScheduledGarnishmentGroupEngine()
    {
        return new ScheduledGarnishmentGroupEngine(
            new List<IScheduledGarnishmentGroupStep>
            {
                new CapScheduledGarnishmentGroupAdjuster(2440.0),
                new ScheduledGarnishmentGroupMaximumOffsetAdjuster(1.41),
                new MinimumScheduledGarnishmentGroupAdjuster(28.9),
            },
            new List<string>(),
            4.2,
            51422.8);
    }

    public double Run(ScheduledGarnishmentGroup item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledGarnishmentGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
