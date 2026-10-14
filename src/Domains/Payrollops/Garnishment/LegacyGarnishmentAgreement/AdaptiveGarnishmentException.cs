namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive garnishment exception is in its life cycle.</summary>
public enum AdaptiveGarnishmentExceptionStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a adaptive garnishment exception and its current status.</summary>
public sealed class AdaptiveGarnishmentException(string code, string name, string department, int windowDays, int endsAt, string skill, string badge, int batchSize, AdaptiveGarnishmentExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public int WindowDays { get; set; } = windowDays;
    public int EndsAt { get; set; } = endsAt;
    public string Skill { get; set; } = skill;
    public string Badge { get; set; } = badge;
    public int BatchSize { get; set; } = batchSize;
    public AdaptiveGarnishmentExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive garnishment exception in status idle.</summary>
    public static AdaptiveGarnishmentException NewAdaptiveGarnishmentException(string code, string name)
    {
        return new AdaptiveGarnishmentException(
            code,
            name,
            "",
            0,
            0,
            "",
            "",
            0,
            AdaptiveGarnishmentExceptionStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveGarnishmentExceptionStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveGarnishmentExceptionStatus next)
    {
        switch (Status)
        {
            case AdaptiveGarnishmentExceptionStatus.Idle:
                switch (next)
                {
                    case AdaptiveGarnishmentExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveGarnishmentExceptionStatus.Warming:
                return next == AdaptiveGarnishmentExceptionStatus.Ready || next == AdaptiveGarnishmentExceptionStatus.Draining;
            case AdaptiveGarnishmentExceptionStatus.Ready:
                switch (next)
                {
                    case AdaptiveGarnishmentExceptionStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveGarnishmentExceptionStatus.Draining:
                return next == AdaptiveGarnishmentExceptionStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveGarnishmentExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive garnishment exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adaptive garnishment exception"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)EndsAt * (double)BatchSize, 2);
    }

    public AdaptiveGarnishmentException Copy()
    {
        return new AdaptiveGarnishmentException(
            Code,
            Name,
            Department,
            WindowDays,
            EndsAt,
            Skill,
            Badge,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
