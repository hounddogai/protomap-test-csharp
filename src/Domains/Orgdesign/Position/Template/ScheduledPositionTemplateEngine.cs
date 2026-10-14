namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled position template calculation.</summary>
public interface IScheduledPositionTemplateAdjuster
{
    string Describe();
    double Adjust(ScheduledPositionTemplate item, double valueValue);
}

public sealed record ScheduledPositionTemplateRetriesOffsetAdjuster(double Weight) : IScheduledPositionTemplateAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(ScheduledPositionTemplate item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record CapScheduledPositionTemplateAdjuster(double Cap) : IScheduledPositionTemplateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledPositionTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScheduledPositionTemplateAdjuster(double Step) : IScheduledPositionTemplateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledPositionTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled position template steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPositionTemplateEngine(List<IScheduledPositionTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPositionTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPositionTemplateEngine DefaultScheduledPositionTemplateEngine()
    {
        return new ScheduledPositionTemplateEngine(
            new List<IScheduledPositionTemplateAdjuster>
            {
                new ScheduledPositionTemplateRetriesOffsetAdjuster(1.22),
                new CapScheduledPositionTemplateAdjuster(45888.0),
                new RoundingScheduledPositionTemplateAdjuster(0.25),
            },
            new List<string>(),
            0.1,
            23563.2);
    }

    public double Price(ScheduledPositionTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPositionTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
