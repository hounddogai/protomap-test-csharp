namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient retro pay exception is in its life cycle.</summary>
public enum LenientRetroPayExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A lenient retro pay exception. Its code is unique within a tenant.</summary>
public sealed class LenientRetroPayException(string code, string label, string country, string source, int tenureMonths, string grade, bool active, int updatedAt, int batchSize, bool pinned, LenientRetroPayExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public string Source { get; set; } = source;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Grade { get; set; } = grade;
    public bool Active { get; set; } = active;
    public int UpdatedAt { get; set; } = updatedAt;
    public int BatchSize { get; set; } = batchSize;
    public bool Pinned { get; set; } = pinned;
    public LenientRetroPayExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a lenient retro pay exception in status proposed.</summary>
    public static LenientRetroPayException NewLenientRetroPayException(string code, string label)
    {
        return new LenientRetroPayException(
            code,
            label,
            "",
            "",
            0,
            "",
            false,
            0,
            0,
            false,
            LenientRetroPayExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientRetroPayExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientRetroPayExceptionStatus next)
    {
        switch (Status)
        {
            case LenientRetroPayExceptionStatus.Proposed:
                return next == LenientRetroPayExceptionStatus.Withdrawn;
            case LenientRetroPayExceptionStatus.Approved:
                return next == LenientRetroPayExceptionStatus.Rejected || next == LenientRetroPayExceptionStatus.Withdrawn;
            case LenientRetroPayExceptionStatus.Rejected:
                return next == LenientRetroPayExceptionStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientRetroPayExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient retro pay exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)TenureMonths + (double)UpdatedAt, 2);
    }

    public LenientRetroPayException Copy()
    {
        return new LenientRetroPayException(
            Code,
            Label,
            Country,
            Source,
            TenureMonths,
            Grade,
            Active,
            UpdatedAt,
            BatchSize,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
