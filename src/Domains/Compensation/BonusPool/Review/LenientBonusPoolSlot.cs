namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient bonus pool slot is in its life cycle.</summary>
public enum LenientBonusPoolSlotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One lenient bonus pool slot, with what the planners need to act on it.</summary>
public sealed class LenientBonusPoolSlot(string code, string title, double hours, double floorValue, string department, double rating, int revision, string grade, double threshold, string employeeId, int startsAt, LenientBonusPoolSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public double FloorValue { get; set; } = floorValue;
    public string Department { get; set; } = department;
    public double Rating { get; set; } = rating;
    public int Revision { get; set; } = revision;
    public string Grade { get; set; } = grade;
    public double Threshold { get; set; } = threshold;
    public string EmployeeId { get; set; } = employeeId;
    public int StartsAt { get; set; } = startsAt;
    public LenientBonusPoolSlotStatus Status { get; set; } = status;

    /// <summary>Starts a lenient bonus pool slot in status created.</summary>
    public static LenientBonusPoolSlot NewLenientBonusPoolSlot(string code, string title)
    {
        return new LenientBonusPoolSlot(
            code,
            title,
            0.0,
            0.0,
            "",
            0.0,
            0,
            "",
            0.0,
            "",
            0,
            LenientBonusPoolSlotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBonusPoolSlotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBonusPoolSlotStatus next)
    {
        switch (Status)
        {
            case LenientBonusPoolSlotStatus.Created:
                switch (next)
                {
                    case LenientBonusPoolSlotStatus.Retired:
                        return true;
                }
                return false;
            case LenientBonusPoolSlotStatus.Validated:
                return next == LenientBonusPoolSlotStatus.Published || next == LenientBonusPoolSlotStatus.Retired;
            case LenientBonusPoolSlotStatus.Published:
                switch (next)
                {
                    case LenientBonusPoolSlotStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientBonusPoolSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient bonus pool slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"lenient bonus pool slot"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(FloorValue - (double)Revision, 2);
    }

    public LenientBonusPoolSlot Copy()
    {
        return new LenientBonusPoolSlot(
            Code,
            Title,
            Hours,
            FloorValue,
            Department,
            Rating,
            Revision,
            Grade,
            Threshold,
            EmployeeId,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
