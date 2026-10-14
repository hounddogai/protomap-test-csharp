namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band schedule is in its life cycle.</summary>
public enum PayBandScheduleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a pay band schedule and its current status.</summary>
public sealed class PayBandSchedule(string code, string label, double threshold, int version, string description, string department, double floorValue, string locale, PayBandScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public int Version { get; set; } = version;
    public string Description { get; set; } = description;
    public string Department { get; set; } = department;
    public double FloorValue { get; set; } = floorValue;
    public string Locale { get; set; } = locale;
    public PayBandScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a pay band schedule in status open.</summary>
    public static PayBandSchedule NewPayBandSchedule(string code, string label)
    {
        return new PayBandSchedule(code, label, 0.0, 0, "", "", 0.0, "", PayBandScheduleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandScheduleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandScheduleStatus next)
    {
        switch (Status)
        {
            case PayBandScheduleStatus.Open:
                switch (next)
                {
                    case PayBandScheduleStatus.Resolved:
                    case PayBandScheduleStatus.Reopened:
                        return true;
                }
                return false;
            case PayBandScheduleStatus.InReview:
                return next == PayBandScheduleStatus.Archived;
            case PayBandScheduleStatus.Resolved:
                return next == PayBandScheduleStatus.Reopened || next == PayBandScheduleStatus.Archived;
            case PayBandScheduleStatus.Reopened:
                return next == PayBandScheduleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PayBandScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveValue()
    {
        return Math.Round(FloorValue * Threshold, 2);
    }

    public PayBandSchedule Copy()
    {
        return new PayBandSchedule(
            Code,
            Label,
            Threshold,
            Version,
            Description,
            Department,
            FloorValue,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
