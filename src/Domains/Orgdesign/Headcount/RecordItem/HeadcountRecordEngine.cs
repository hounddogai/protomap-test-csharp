namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount record calculation.</summary>
public interface IHeadcountRecordFactor
{
    string Describe();
    double Adjust(HeadcountRecord item, double valueValue);
}

public sealed record HeadcountRecordTenureMonthsOffsetAdjuster(double Weight) : IHeadcountRecordFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(HeadcountRecord item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record RoundingHeadcountRecordAdjuster(double Step) : IHeadcountRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapHeadcountRecordAdjuster(double Cap) : IHeadcountRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumHeadcountRecordAdjuster(double Minimum) : IHeadcountRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the headcount record steps in order and keeps a trace of each.</summary>
public sealed class HeadcountRecordEngine(List<IHeadcountRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountRecordEngine DefaultHeadcountRecordEngine()
    {
        return new HeadcountRecordEngine(
            new List<IHeadcountRecordFactor>
            {
                new HeadcountRecordTenureMonthsOffsetAdjuster(0.34),
                new RoundingHeadcountRecordAdjuster(0.1),
                new CapHeadcountRecordAdjuster(4181.0),
                new MinimumHeadcountRecordAdjuster(5.9),
            },
            new List<string>(),
            1.2,
            68071.8);
    }

    public double Price(HeadcountRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
