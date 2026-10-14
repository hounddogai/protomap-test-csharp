namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled adjustment reservation is in its life cycle.</summary>
public enum ScheduledAdjustmentReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a scheduled adjustment reservation and its current status.</summary>
public sealed class ScheduledAdjustmentReservation(string code, string title, List<string> tags, int priority, string phone, string reviewer, string ownerId, double ceiling, int graceDays, string channel, ScheduledAdjustmentReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public int Priority { get; set; } = priority;
    public string Phone { get; set; } = phone;
    public string Reviewer { get; set; } = reviewer;
    public string OwnerId { get; set; } = ownerId;
    public double Ceiling { get; set; } = ceiling;
    public int GraceDays { get; set; } = graceDays;
    public string Channel { get; set; } = channel;
    public ScheduledAdjustmentReservationStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled adjustment reservation in status requested.</summary>
    public static ScheduledAdjustmentReservation NewScheduledAdjustmentReservation(string code, string title)
    {
        return new ScheduledAdjustmentReservation(
            code,
            title,
            new List<string>(),
            0,
            "",
            "",
            "",
            0.0,
            0,
            "",
            ScheduledAdjustmentReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAdjustmentReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAdjustmentReservationStatus next)
    {
        switch (Status)
        {
            case ScheduledAdjustmentReservationStatus.Requested:
                return next == ScheduledAdjustmentReservationStatus.Revoked || next == ScheduledAdjustmentReservationStatus.Expired;
            case ScheduledAdjustmentReservationStatus.Granted:
                switch (next)
                {
                    case ScheduledAdjustmentReservationStatus.Revoked:
                        return true;
                }
                return false;
            case ScheduledAdjustmentReservationStatus.Revoked:
                switch (next)
                {
                    case ScheduledAdjustmentReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledAdjustmentReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled adjustment reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)GraceDays - Ceiling, 2);
    }

    public ScheduledAdjustmentReservation Copy()
    {
        return new ScheduledAdjustmentReservation(
            Code,
            Title,
            Tags,
            Priority,
            Phone,
            Reviewer,
            OwnerId,
            Ceiling,
            GraceDays,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
