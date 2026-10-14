namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient training due schedule is in its life cycle.</summary>
public enum LenientTrainingDueScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One lenient training due schedule, with what the planners need to act on it.</summary>
public sealed class LenientTrainingDueSchedule(string code, string label, double discount, string summary, int capacity, string segment, string timezone, string locale, int createdAt, double fte, double floorValue, LenientTrainingDueScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public string Summary { get; set; } = summary;
    public int Capacity { get; set; } = capacity;
    public string Segment { get; set; } = segment;
    public string Timezone { get; set; } = timezone;
    public string Locale { get; set; } = locale;
    public int CreatedAt { get; set; } = createdAt;
    public double Fte { get; set; } = fte;
    public double FloorValue { get; set; } = floorValue;
    public LenientTrainingDueScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient training due schedule in status planned.</summary>
    public static LenientTrainingDueSchedule NewLenientTrainingDueSchedule(string code, string label)
    {
        return new LenientTrainingDueSchedule(
            code,
            label,
            0.0,
            "",
            0,
            "",
            "",
            "",
            0,
            0.0,
            0.0,
            LenientTrainingDueScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientTrainingDueScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientTrainingDueScheduleStatus next)
    {
        switch (Status)
        {
            case LenientTrainingDueScheduleStatus.Planned:
                switch (next)
                {
                    case LenientTrainingDueScheduleStatus.Scheduled:
                        return true;
                }
                return false;
            case LenientTrainingDueScheduleStatus.Scheduled:
                switch (next)
                {
                    case LenientTrainingDueScheduleStatus.InProgress:
                        return true;
                }
                return false;
            case LenientTrainingDueScheduleStatus.InProgress:
                return next == LenientTrainingDueScheduleStatus.Cancelled;
            case LenientTrainingDueScheduleStatus.Completed:
                switch (next)
                {
                    case LenientTrainingDueScheduleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientTrainingDueScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient training due schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)CreatedAt * Fte, 2);
    }

    public LenientTrainingDueSchedule Copy()
    {
        return new LenientTrainingDueSchedule(
            Code,
            Label,
            Discount,
            Summary,
            Capacity,
            Segment,
            Timezone,
            Locale,
            CreatedAt,
            Fte,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
