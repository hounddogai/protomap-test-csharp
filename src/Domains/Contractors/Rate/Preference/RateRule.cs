namespace Acme.Workforce.Contractors.Rate.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate rule is in its life cycle.</summary>
public enum RateRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a rate rule and its current status.</summary>
public sealed class RateRule(string code, string name, double ratio, string country, string locale, string source, RateRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public string Source { get; set; } = source;
    public RateRuleStatus Status { get; set; } = status;

    /// <summary>Starts a rate rule in status new.</summary>
    public static RateRule NewRateRule(string code, string name)
    {
        return new RateRule(code, name, 0.0, "", "", "", RateRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateRuleStatus next)
    {
        switch (Status)
        {
            case RateRuleStatus.New:
                return next == RateRuleStatus.Queued;
            case RateRuleStatus.Queued:
                switch (next)
                {
                    case RateRuleStatus.Done:
                    case RateRuleStatus.Failed:
                        return true;
                }
                return false;
            case RateRuleStatus.Running:
                switch (next)
                {
                    case RateRuleStatus.Done:
                    case RateRuleStatus.Failed:
                        return true;
                }
                return false;
            case RateRuleStatus.Done:
                return next == RateRuleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RateRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public RateRule Copy()
    {
        return new RateRule(Code, Name, Ratio, Country, Locale, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
