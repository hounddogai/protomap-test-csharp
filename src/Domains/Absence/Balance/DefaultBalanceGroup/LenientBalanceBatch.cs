namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient balance batch is in its life cycle.</summary>
public enum LenientBalanceBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient balance batch. Its code is unique within a tenant.</summary>
public sealed class LenientBalanceBatch(string code, string title, int endsAt, string externalRef, int graceDays, double hours, string contactEmail, string notes, string grade, double amount, double minimum, LenientBalanceBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public string ExternalRef { get; set; } = externalRef;
    public int GraceDays { get; set; } = graceDays;
    public double Hours { get; set; } = hours;
    public string ContactEmail { get; set; } = contactEmail;
    public string Notes { get; set; } = notes;
    public string Grade { get; set; } = grade;
    public double Amount { get; set; } = amount;
    public double Minimum { get; set; } = minimum;
    public LenientBalanceBatchStatus Status { get; set; } = status;

    /// <summary>Starts a lenient balance batch in status planned.</summary>
    public static LenientBalanceBatch NewLenientBalanceBatch(string code, string title)
    {
        return new LenientBalanceBatch(
            code,
            title,
            0,
            "",
            0,
            0.0,
            "",
            "",
            "",
            0.0,
            0.0,
            LenientBalanceBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBalanceBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBalanceBatchStatus next)
    {
        switch (Status)
        {
            case LenientBalanceBatchStatus.Planned:
                return next == LenientBalanceBatchStatus.Scheduled || next == LenientBalanceBatchStatus.Cancelled;
            case LenientBalanceBatchStatus.Scheduled:
                switch (next)
                {
                    case LenientBalanceBatchStatus.Completed:
                        return true;
                }
                return false;
            case LenientBalanceBatchStatus.InProgress:
                return next == LenientBalanceBatchStatus.Completed;
            case LenientBalanceBatchStatus.Completed:
                switch (next)
                {
                    case LenientBalanceBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBalanceBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient balance batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"lenient balance batch"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Minimum - (double)GraceDays, 2);
    }

    public LenientBalanceBatch Copy()
    {
        return new LenientBalanceBatch(
            Code,
            Title,
            EndsAt,
            ExternalRef,
            GraceDays,
            Hours,
            ContactEmail,
            Notes,
            Grade,
            Amount,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
