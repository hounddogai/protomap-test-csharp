namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk cycle calculation.</summary>
public interface IDeskCycleModifier
{
    string Describe();
    double Adjust(DeskCycle item, double valueValue);
}

public sealed record DeskCycleAmountOffsetAdjuster(double Weight) : IDeskCycleModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(DeskCycle item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record TitleDeskCycleAdjuster(Dictionary<string, double> Factors) : IDeskCycleModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DeskCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDeskCycleAdjuster(double Percent) : IDeskCycleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDeskCycleAdjuster(double Minimum) : IDeskCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeskCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingDeskCycleAdjuster(double Step) : IDeskCycleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the desk cycle steps in order and keeps a trace of each.</summary>
public sealed class DeskCycleEngine(List<IDeskCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskCycleEngine DefaultDeskCycleEngine()
    {
        return new DeskCycleEngine(
            new List<IDeskCycleModifier>
            {
                new DeskCycleAmountOffsetAdjuster(0.65),
                new TitleDeskCycleAdjuster(new Dictionary<string, double> { ["east"] = 0.92, ["south"] = 1.27 }),
                new PercentDeskCycleAdjuster(1.1),
                new MinimumDeskCycleAdjuster(28.7),
                new RoundingDeskCycleAdjuster(0.1),
            },
            new List<string>(),
            3.0,
            23551.2);
    }

    public double Evaluate(DeskCycle item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<DeskCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
