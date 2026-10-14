namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy rate variance is in its life cycle.</summary>
public enum LegacyRateVarianceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One legacy rate variance, with what the planners need to act on it.</summary>
public sealed class LegacyRateVariance(string code, string name, string contactEmail, int priority, double margin, double maximum, int leaveDays, string timezone, double ceiling, string phone, LegacyRateVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public int Priority { get; set; } = priority;
    public double Margin { get; set; } = margin;
    public double Maximum { get; set; } = maximum;
    public int LeaveDays { get; set; } = leaveDays;
    public string Timezone { get; set; } = timezone;
    public double Ceiling { get; set; } = ceiling;
    public string Phone { get; set; } = phone;
    public LegacyRateVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a legacy rate variance in status idle.</summary>
    public static LegacyRateVariance NewLegacyRateVariance(string code, string name)
    {
        return new LegacyRateVariance(code, name, "", 0, 0.0, 0.0, 0, "", 0.0, "", LegacyRateVarianceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRateVarianceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRateVarianceStatus next)
    {
        switch (Status)
        {
            case LegacyRateVarianceStatus.Idle:
                switch (next)
                {
                    case LegacyRateVarianceStatus.Warming:
                    case LegacyRateVarianceStatus.Ready:
                        return true;
                }
                return false;
            case LegacyRateVarianceStatus.Warming:
                return next == LegacyRateVarianceStatus.Draining;
            case LegacyRateVarianceStatus.Ready:
                switch (next)
                {
                    case LegacyRateVarianceStatus.Draining:
                    case LegacyRateVarianceStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyRateVarianceStatus.Draining:
                return next == LegacyRateVarianceStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LegacyRateVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy rate variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Priority - Ceiling, 2);
    }

    public LegacyRateVariance Copy()
    {
        return new LegacyRateVariance(
            Code,
            Name,
            ContactEmail,
            Priority,
            Margin,
            Maximum,
            LeaveDays,
            Timezone,
            Ceiling,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
