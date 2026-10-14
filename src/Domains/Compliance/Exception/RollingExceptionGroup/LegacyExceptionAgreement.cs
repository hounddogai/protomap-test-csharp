namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy exception agreement is in its life cycle.</summary>
public enum LegacyExceptionAgreementStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a legacy exception agreement and its current status.</summary>
public sealed class LegacyExceptionAgreement(string code, string name, double threshold, string skill, string title, string locale, int sequence, string contactEmail, int graceDays, int durationMinutes, LegacyExceptionAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string Skill { get; set; } = skill;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public int Sequence { get; set; } = sequence;
    public string ContactEmail { get; set; } = contactEmail;
    public int GraceDays { get; set; } = graceDays;
    public int DurationMinutes { get; set; } = durationMinutes;
    public LegacyExceptionAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a legacy exception agreement in status idle.</summary>
    public static LegacyExceptionAgreement NewLegacyExceptionAgreement(string code, string name)
    {
        return new LegacyExceptionAgreement(
            code,
            name,
            0.0,
            "",
            "",
            "",
            0,
            "",
            0,
            0,
            LegacyExceptionAgreementStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyExceptionAgreementStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyExceptionAgreementStatus next)
    {
        switch (Status)
        {
            case LegacyExceptionAgreementStatus.Idle:
                switch (next)
                {
                    case LegacyExceptionAgreementStatus.Warming:
                    case LegacyExceptionAgreementStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyExceptionAgreementStatus.Warming:
                switch (next)
                {
                    case LegacyExceptionAgreementStatus.Ready:
                    case LegacyExceptionAgreementStatus.Draining:
                        return true;
                }
                return false;
            case LegacyExceptionAgreementStatus.Ready:
                switch (next)
                {
                    case LegacyExceptionAgreementStatus.Draining:
                    case LegacyExceptionAgreementStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyExceptionAgreementStatus.Draining:
                switch (next)
                {
                    case LegacyExceptionAgreementStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyExceptionAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy exception agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)GraceDays * Threshold, 2);
    }

    public LegacyExceptionAgreement Copy()
    {
        return new LegacyExceptionAgreement(
            Code,
            Name,
            Threshold,
            Skill,
            Title,
            Locale,
            Sequence,
            ContactEmail,
            GraceDays,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
