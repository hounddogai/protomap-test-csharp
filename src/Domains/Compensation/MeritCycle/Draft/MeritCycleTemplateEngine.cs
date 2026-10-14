namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle template calculation.</summary>
public interface IMeritCycleTemplateModifier
{
    string Describe();
    double Adjust(MeritCycleTemplate item, double valueValue);
}

public sealed record CapMeritCycleTemplateAdjuster(double Cap) : IMeritCycleTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentMeritCycleTemplateAdjuster(double Percent) : IMeritCycleTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingMeritCycleTemplateAdjuster(double Step) : IMeritCycleTemplateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MeritCycleTemplateLeaveDaysOffsetAdjuster(double Weight) : IMeritCycleTemplateModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(MeritCycleTemplate item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record MinimumMeritCycleTemplateAdjuster(double Minimum) : IMeritCycleTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the merit cycle template steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleTemplateEngine(List<IMeritCycleTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleTemplateEngine DefaultMeritCycleTemplateEngine()
    {
        return new MeritCycleTemplateEngine(
            new List<IMeritCycleTemplateModifier>
            {
                new CapMeritCycleTemplateAdjuster(4698.0),
                new PercentMeritCycleTemplateAdjuster(-11.8),
                new RoundingMeritCycleTemplateAdjuster(0.05),
                new MeritCycleTemplateLeaveDaysOffsetAdjuster(1.05),
                new MinimumMeritCycleTemplateAdjuster(23.8),
            },
            new List<string>(),
            0.4,
            89316.1);
    }

    public double Evaluate(MeritCycleTemplate item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
