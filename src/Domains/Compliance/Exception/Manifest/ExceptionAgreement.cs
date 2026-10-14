namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception agreement is in its life cycle.</summary>
public enum ExceptionAgreementStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A exception agreement. Its code is unique within a tenant.</summary>
public sealed class ExceptionAgreement(string code, string title, int durationMinutes, int retries, int attempts, ExceptionAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Retries { get; set; } = retries;
    public int Attempts { get; set; } = attempts;
    public ExceptionAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a exception agreement in status proposed.</summary>
    public static ExceptionAgreement NewExceptionAgreement(string code, string title)
    {
        return new ExceptionAgreement(code, title, 0, 0, 0, ExceptionAgreementStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionAgreementStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionAgreementStatus next)
    {
        switch (Status)
        {
            case ExceptionAgreementStatus.Proposed:
                return next == ExceptionAgreementStatus.Approved;
            case ExceptionAgreementStatus.Approved:
                return next == ExceptionAgreementStatus.Rejected;
            case ExceptionAgreementStatus.Rejected:
                return next == ExceptionAgreementStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ExceptionAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"exception agreement"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Attempts + (double)Retries, 2);
    }

    public ExceptionAgreement Copy()
    {
        return new ExceptionAgreement(Code, Title, DurationMinutes, Retries, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
