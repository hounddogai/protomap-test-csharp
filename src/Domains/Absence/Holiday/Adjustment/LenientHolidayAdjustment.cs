namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient holiday adjustment is in its life cycle.</summary>
public enum LenientHolidayAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A lenient holiday adjustment. Its code is unique within a tenant.</summary>
public sealed class LenientHolidayAdjustment(string code, string name, string category, double score, bool archived, string notes, string contactEmail, LenientHolidayAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public string Notes { get; set; } = notes;
    public string ContactEmail { get; set; } = contactEmail;
    public LenientHolidayAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a lenient holiday adjustment in status requested.</summary>
    public static LenientHolidayAdjustment NewLenientHolidayAdjustment(string code, string name)
    {
        return new LenientHolidayAdjustment(
            code,
            name,
            "",
            0.0,
            false,
            "",
            "",
            LenientHolidayAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientHolidayAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientHolidayAdjustmentStatus next)
    {
        switch (Status)
        {
            case LenientHolidayAdjustmentStatus.Requested:
                switch (next)
                {
                    case LenientHolidayAdjustmentStatus.Expired:
                        return true;
                }
                return false;
            case LenientHolidayAdjustmentStatus.Granted:
                return next == LenientHolidayAdjustmentStatus.Revoked;
            case LenientHolidayAdjustmentStatus.Revoked:
                switch (next)
                {
                    case LenientHolidayAdjustmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientHolidayAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient holiday adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public LenientHolidayAdjustment Copy()
    {
        return new LenientHolidayAdjustment(Code, Name, Category, Score, Archived, Notes, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
