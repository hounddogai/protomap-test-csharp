namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle draft calculation.</summary>
public interface IMeritCycleDraftAdjuster
{
    string Describe();
    double Adjust(MeritCycleDraft item, double valueValue);
}

public sealed record RoundingMeritCycleDraftAdjuster(double Step) : IMeritCycleDraftAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MeritCycleDraftVersionOffsetAdjuster(double Weight) : IMeritCycleDraftAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(MeritCycleDraft item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumMeritCycleDraftAdjuster(double Minimum) : IMeritCycleDraftAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the merit cycle draft steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleDraftEngine(List<IMeritCycleDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleDraftEngine DefaultMeritCycleDraftEngine()
    {
        return new MeritCycleDraftEngine(
            new List<IMeritCycleDraftAdjuster>
            {
                new RoundingMeritCycleDraftAdjuster(0.1),
                new MeritCycleDraftVersionOffsetAdjuster(0.21),
                new MinimumMeritCycleDraftAdjuster(9.3),
            },
            new List<string>(),
            1.8,
            58273.8);
    }

    public double Run(MeritCycleDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
