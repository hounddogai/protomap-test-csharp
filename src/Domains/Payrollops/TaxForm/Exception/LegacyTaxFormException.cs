namespace Acme.Workforce.Payrollops.TaxForm.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy tax form exception is in its life cycle.</summary>
public enum LegacyTaxFormExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One legacy tax form exception, with what the planners need to act on it.</summary>
public sealed class LegacyTaxFormException(string code, string title, int attempts, string jobCode, string managerId, bool approved, int endsAt, string salaryBand, int durationMinutes, LegacyTaxFormExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public string JobCode { get; set; } = jobCode;
    public string ManagerId { get; set; } = managerId;
    public bool Approved { get; set; } = approved;
    public int EndsAt { get; set; } = endsAt;
    public string SalaryBand { get; set; } = salaryBand;
    public int DurationMinutes { get; set; } = durationMinutes;
    public LegacyTaxFormExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a legacy tax form exception in status planned.</summary>
    public static LegacyTaxFormException NewLegacyTaxFormException(string code, string title)
    {
        return new LegacyTaxFormException(
            code,
            title,
            0,
            "",
            "",
            false,
            0,
            "",
            0,
            LegacyTaxFormExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTaxFormExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTaxFormExceptionStatus next)
    {
        switch (Status)
        {
            case LegacyTaxFormExceptionStatus.Planned:
                switch (next)
                {
                    case LegacyTaxFormExceptionStatus.InProgress:
                    case LegacyTaxFormExceptionStatus.Completed:
                        return true;
                }
                return false;
            case LegacyTaxFormExceptionStatus.Scheduled:
                return next == LegacyTaxFormExceptionStatus.Completed;
            case LegacyTaxFormExceptionStatus.InProgress:
                return next == LegacyTaxFormExceptionStatus.Completed || next == LegacyTaxFormExceptionStatus.Cancelled;
            case LegacyTaxFormExceptionStatus.Completed:
                return next == LegacyTaxFormExceptionStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LegacyTaxFormExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy tax form exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Attempts - (double)EndsAt, 2);
    }

    public LegacyTaxFormException Copy()
    {
        return new LegacyTaxFormException(
            Code,
            Title,
            Attempts,
            JobCode,
            ManagerId,
            Approved,
            EndsAt,
            SalaryBand,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
