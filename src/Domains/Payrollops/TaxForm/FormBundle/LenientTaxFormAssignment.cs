namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient tax form assignment is in its life cycle.</summary>
public enum LenientTaxFormAssignmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient tax form assignment as the tax form team keeps it.</summary>
public sealed class LenientTaxFormAssignment(string code, string title, string jobCode, string notes, double minimum, double ceiling, int expiresAt, string region, LenientTaxFormAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public string Notes { get; set; } = notes;
    public double Minimum { get; set; } = minimum;
    public double Ceiling { get; set; } = ceiling;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Region { get; set; } = region;
    public LenientTaxFormAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a lenient tax form assignment in status planned.</summary>
    public static LenientTaxFormAssignment NewLenientTaxFormAssignment(string code, string title)
    {
        return new LenientTaxFormAssignment(
            code,
            title,
            "",
            "",
            0.0,
            0.0,
            0,
            "",
            LenientTaxFormAssignmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientTaxFormAssignmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientTaxFormAssignmentStatus next)
    {
        switch (Status)
        {
            case LenientTaxFormAssignmentStatus.Planned:
                switch (next)
                {
                    case LenientTaxFormAssignmentStatus.Scheduled:
                        return true;
                }
                return false;
            case LenientTaxFormAssignmentStatus.Scheduled:
                switch (next)
                {
                    case LenientTaxFormAssignmentStatus.Completed:
                    case LenientTaxFormAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientTaxFormAssignmentStatus.InProgress:
                return next == LenientTaxFormAssignmentStatus.Completed || next == LenientTaxFormAssignmentStatus.Cancelled;
            case LenientTaxFormAssignmentStatus.Completed:
                switch (next)
                {
                    case LenientTaxFormAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientTaxFormAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient tax form assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Ceiling - Minimum, 2);
    }

    public LenientTaxFormAssignment Copy()
    {
        return new LenientTaxFormAssignment(
            Code,
            Title,
            JobCode,
            Notes,
            Minimum,
            Ceiling,
            ExpiresAt,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
