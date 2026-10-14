namespace Acme.Workforce.Absence.Holiday.Review;

/// <summary>One step of the holiday snapshot calculation.</summary>
public interface IHolidaySnapshotModifier
{
    string Describe();
    double Adjust(HolidaySnapshot item, double valueValue);
}

public sealed record MinimumHolidaySnapshotAdjuster(double Minimum) : IHolidaySnapshotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidaySnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CountryHolidaySnapshotAdjuster(Dictionary<string, double> Factors) : IHolidaySnapshotModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(HolidaySnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapHolidaySnapshotAdjuster(double Cap) : IHolidaySnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidaySnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the holiday snapshot steps in order and keeps a trace of each.</summary>
public sealed class HolidaySnapshotEngine(List<IHolidaySnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidaySnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidaySnapshotEngine DefaultHolidaySnapshotEngine()
    {
        return new HolidaySnapshotEngine(
            new List<IHolidaySnapshotModifier>
            {
                new MinimumHolidaySnapshotAdjuster(30.7),
                new CountryHolidaySnapshotAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.94, ["south"] = 1.35, ["west"] = 0.73 }),
                new CapHolidaySnapshotAdjuster(44403.0),
            },
            new List<string>(),
            4.9,
            39168.6);
    }

    public double Price(HolidaySnapshot item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<HolidaySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
