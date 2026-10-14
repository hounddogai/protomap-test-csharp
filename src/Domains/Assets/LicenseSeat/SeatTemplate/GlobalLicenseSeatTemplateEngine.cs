namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global license seat template calculation.</summary>
public interface IGlobalLicenseSeatTemplateModifier
{
    string Describe();
    double Adjust(GlobalLicenseSeatTemplate item, double valueValue);
}

public sealed record NameGlobalLicenseSeatTemplateAdjuster(Dictionary<string, double> Factors) : IGlobalLicenseSeatTemplateModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(GlobalLicenseSeatTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapGlobalLicenseSeatTemplateAdjuster(double Cap) : IGlobalLicenseSeatTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalLicenseSeatTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumGlobalLicenseSeatTemplateAdjuster(double Minimum) : IGlobalLicenseSeatTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalLicenseSeatTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the global license seat template steps in order and keeps a trace of each.</summary>
public sealed class GlobalLicenseSeatTemplateEngine(List<IGlobalLicenseSeatTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalLicenseSeatTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalLicenseSeatTemplateEngine DefaultGlobalLicenseSeatTemplateEngine()
    {
        return new GlobalLicenseSeatTemplateEngine(
            new List<IGlobalLicenseSeatTemplateModifier>
            {
                new NameGlobalLicenseSeatTemplateAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.32, ["north"] = 1.15, ["central"] = 0.95 }),
                new CapGlobalLicenseSeatTemplateAdjuster(44554.0),
                new MinimumGlobalLicenseSeatTemplateAdjuster(1.3),
            },
            new List<string>(),
            4.7,
            13858.6);
    }

    public double Evaluate(GlobalLicenseSeatTemplate item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalLicenseSeatTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
