namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending deduction exception is in its life cycle.</summary>
public enum PendingDeductionExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pending deduction exception as the deduction team keeps it.</summary>
public sealed class PendingDeductionException(string code, string label, string country, int sequence, int endsAt, string externalRef, int batchSize, int leaveDays, string channel, string contactEmail, double minimum, PendingDeductionExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public int Sequence { get; set; } = sequence;
    public int EndsAt { get; set; } = endsAt;
    public string ExternalRef { get; set; } = externalRef;
    public int BatchSize { get; set; } = batchSize;
    public int LeaveDays { get; set; } = leaveDays;
    public string Channel { get; set; } = channel;
    public string ContactEmail { get; set; } = contactEmail;
    public double Minimum { get; set; } = minimum;
    public PendingDeductionExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a pending deduction exception in status planned.</summary>
    public static PendingDeductionException NewPendingDeductionException(string code, string label)
    {
        return new PendingDeductionException(
            code,
            label,
            "",
            0,
            0,
            "",
            0,
            0,
            "",
            "",
            0.0,
            PendingDeductionExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingDeductionExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingDeductionExceptionStatus next)
    {
        switch (Status)
        {
            case PendingDeductionExceptionStatus.Planned:
                return next == PendingDeductionExceptionStatus.Completed;
            case PendingDeductionExceptionStatus.Scheduled:
                switch (next)
                {
                    case PendingDeductionExceptionStatus.Completed:
                    case PendingDeductionExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingDeductionExceptionStatus.InProgress:
                return next == PendingDeductionExceptionStatus.Cancelled;
            case PendingDeductionExceptionStatus.Completed:
                return next == PendingDeductionExceptionStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PendingDeductionExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending deduction exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)EndsAt + (double)LeaveDays, 2);
    }

    public PendingDeductionException Copy()
    {
        return new PendingDeductionException(
            Code,
            Label,
            Country,
            Sequence,
            EndsAt,
            ExternalRef,
            BatchSize,
            LeaveDays,
            Channel,
            ContactEmail,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
