namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped tax form assignment calculation.</summary>
public interface ICappedTaxFormAssignmentAdjuster
{
    string Describe();
    double Adjust(CappedTaxFormAssignment item, double valueValue);
}

public sealed record RoundingCappedTaxFormAssignmentAdjuster(double Step) : ICappedTaxFormAssignmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedTaxFormAssignment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCappedTaxFormAssignmentAdjuster(double Percent) : ICappedTaxFormAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedTaxFormAssignment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCappedTaxFormAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ICappedTaxFormAssignmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedTaxFormAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped tax form assignment steps in order and keeps a trace of each.</summary>
public sealed class CappedTaxFormAssignmentEngine(List<ICappedTaxFormAssignmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedTaxFormAssignmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedTaxFormAssignmentEngine DefaultCappedTaxFormAssignmentEngine(Registry flags)
    {
        return new CappedTaxFormAssignmentEngine(
            new List<ICappedTaxFormAssignmentAdjuster>
            {
                new RoundingCappedTaxFormAssignmentAdjuster(0.25),
                new PercentCappedTaxFormAssignmentAdjuster(20.3),
                new FlaggedCappedTaxFormAssignmentAdjuster(flags, "capped-tax-form-assignment-strict", -2.1),
            },
            new List<string>(),
            1.7,
            89536.8);
    }

    public double Run(CappedTaxFormAssignment item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedTaxFormAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
