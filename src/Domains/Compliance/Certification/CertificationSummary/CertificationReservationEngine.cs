namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the certification reservation calculation.</summary>
public interface ICertificationReservationModifier
{
    string Describe();
    double Adjust(CertificationReservation item, double valueValue);
}

public sealed record LabelCertificationReservationAdjuster(Dictionary<string, double> Factors) : ICertificationReservationModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CertificationReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumCertificationReservationAdjuster(double Minimum) : ICertificationReservationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CertificationReservationLeaveDaysOffsetAdjuster(double Weight) : ICertificationReservationModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(CertificationReservation item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record FlaggedCertificationReservationAdjuster(Registry Flags, string Flag, double Percent) : ICertificationReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CertificationReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCertificationReservationAdjuster(double Cap) : ICertificationReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the certification reservation steps in order and keeps a trace of each.</summary>
public sealed class CertificationReservationEngine(List<ICertificationReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationReservationEngine DefaultCertificationReservationEngine(Registry flags)
    {
        return new CertificationReservationEngine(
            new List<ICertificationReservationModifier>
            {
                new LabelCertificationReservationAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.97, ["retail"] = 1.27, ["central"] = 0.83 }),
                new MinimumCertificationReservationAdjuster(44.5),
                new CertificationReservationLeaveDaysOffsetAdjuster(0.24),
                new FlaggedCertificationReservationAdjuster(flags, "certification-reservation-capped", -1.7),
                new CapCertificationReservationAdjuster(24018.0),
            },
            new List<string>(),
            4.9,
            76988.3);
    }

    public double Compute(CertificationReservation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
