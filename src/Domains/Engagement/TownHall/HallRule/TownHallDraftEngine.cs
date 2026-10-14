namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall draft calculation.</summary>
public interface ITownHallDraftModifier
{
    string Describe();
    double Adjust(TownHallDraft item, double valueValue);
}

public sealed record TieredTownHallDraftAdjuster(List<double> Bounds, List<double> Rates) : ITownHallDraftModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallDraft item, double valueValue)
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

public sealed record TownHallDraftRevisionOffsetAdjuster(double Weight) : ITownHallDraftModifier
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(TownHallDraft item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record CapTownHallDraftAdjuster(double Cap) : ITownHallDraftModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTownHallDraftAdjuster(double Minimum) : ITownHallDraftModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameTownHallDraftAdjuster(Dictionary<string, double> Factors) : ITownHallDraftModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TownHallDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTownHallDraftAdjuster(double Step) : ITownHallDraftModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the town hall draft steps in order and keeps a trace of each.</summary>
public sealed class TownHallDraftEngine(List<ITownHallDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallDraftEngine DefaultTownHallDraftEngine()
    {
        return new TownHallDraftEngine(
            new List<ITownHallDraftModifier>
            {
                new TieredTownHallDraftAdjuster(new List<double> { 725.0, 1742.0 }, new List<double> { 1.01, 0.81 }),
                new TownHallDraftRevisionOffsetAdjuster(1.47),
                new CapTownHallDraftAdjuster(35239.0),
                new MinimumTownHallDraftAdjuster(34.5),
                new NameTownHallDraftAdjuster(new Dictionary<string, double> { ["retail"] = 0.82, ["west"] = 1.3 }),
                new RoundingTownHallDraftAdjuster(1.0),
            },
            new List<string>(),
            0.2,
            30981.2);
    }

    public double Evaluate(TownHallDraft item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TownHallDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
