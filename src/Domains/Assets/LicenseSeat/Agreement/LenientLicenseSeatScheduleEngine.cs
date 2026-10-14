namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient license seat schedule calculation.</summary>
public interface ILenientLicenseSeatScheduleModifier
{
    string Describe();
    double Adjust(LenientLicenseSeatSchedule item, double valueValue);
}

public sealed record RoundingLenientLicenseSeatScheduleAdjuster(double Step) : ILenientLicenseSeatScheduleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientLicenseSeatSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLenientLicenseSeatScheduleAdjuster(double Minimum) : ILenientLicenseSeatScheduleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientLicenseSeatSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLenientLicenseSeatScheduleAdjuster(double Cap) : ILenientLicenseSeatScheduleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientLicenseSeatSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientLicenseSeatScheduleAdjuster(double Percent) : ILenientLicenseSeatScheduleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientLicenseSeatSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient license seat schedule steps in order and keeps a trace of each.</summary>
public sealed class LenientLicenseSeatScheduleEngine(List<ILenientLicenseSeatScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientLicenseSeatScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientLicenseSeatScheduleEngine DefaultLenientLicenseSeatScheduleEngine()
    {
        return new LenientLicenseSeatScheduleEngine(
            new List<ILenientLicenseSeatScheduleModifier>
            {
                new RoundingLenientLicenseSeatScheduleAdjuster(0.1),
                new MinimumLenientLicenseSeatScheduleAdjuster(42.7),
                new CapLenientLicenseSeatScheduleAdjuster(42715.0),
                new PercentLenientLicenseSeatScheduleAdjuster(-1.5),
            },
            new List<string>(),
            3.9,
            27833.5);
    }

    public double Compute(LenientLicenseSeatSchedule item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientLicenseSeatSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
