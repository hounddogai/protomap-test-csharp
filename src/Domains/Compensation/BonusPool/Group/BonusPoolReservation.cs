namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool reservation is in its life cycle.</summary>
public enum BonusPoolReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One bonus pool reservation, with what the planners need to act on it.</summary>
public sealed class BonusPoolReservation(string code, string label, string badge, string category, double score, int durationMinutes, bool archived, string region, int windowDays, string country, string managerId, BonusPoolReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Category { get; set; } = category;
    public double Score { get; set; } = score;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Archived { get; set; } = archived;
    public string Region { get; set; } = region;
    public int WindowDays { get; set; } = windowDays;
    public string Country { get; set; } = country;
    public string ManagerId { get; set; } = managerId;
    public BonusPoolReservationStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool reservation in status idle.</summary>
    public static BonusPoolReservation NewBonusPoolReservation(string code, string label)
    {
        return new BonusPoolReservation(
            code,
            label,
            "",
            "",
            0.0,
            0,
            false,
            "",
            0,
            "",
            "",
            BonusPoolReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolReservationStatus next)
    {
        switch (Status)
        {
            case BonusPoolReservationStatus.Idle:
                return next == BonusPoolReservationStatus.Draining || next == BonusPoolReservationStatus.Stopped;
            case BonusPoolReservationStatus.Warming:
                switch (next)
                {
                    case BonusPoolReservationStatus.Ready:
                    case BonusPoolReservationStatus.Stopped:
                        return true;
                }
                return false;
            case BonusPoolReservationStatus.Ready:
                switch (next)
                {
                    case BonusPoolReservationStatus.Stopped:
                        return true;
                }
                return false;
            case BonusPoolReservationStatus.Draining:
                switch (next)
                {
                    case BonusPoolReservationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetValue()
    {
        return Math.Round((double)WindowDays - Score, 2);
    }

    public BonusPoolReservation Copy()
    {
        return new BonusPoolReservation(
            Code,
            Label,
            Badge,
            Category,
            Score,
            DurationMinutes,
            Archived,
            Region,
            WindowDays,
            Country,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
