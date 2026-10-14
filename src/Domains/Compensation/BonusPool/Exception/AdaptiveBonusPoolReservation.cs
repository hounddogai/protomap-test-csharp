namespace Acme.Workforce.Compensation.BonusPool.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive bonus pool reservation is in its life cycle.</summary>
public enum AdaptiveBonusPoolReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One adaptive bonus pool reservation, with what the planners need to act on it.</summary>
public sealed class AdaptiveBonusPoolReservation(string code, string label, string unit, string locale, double threshold, int capacity, int windowDays, string badge, string skill, bool archived, int priority, AdaptiveBonusPoolReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Locale { get; set; } = locale;
    public double Threshold { get; set; } = threshold;
    public int Capacity { get; set; } = capacity;
    public int WindowDays { get; set; } = windowDays;
    public string Badge { get; set; } = badge;
    public string Skill { get; set; } = skill;
    public bool Archived { get; set; } = archived;
    public int Priority { get; set; } = priority;
    public AdaptiveBonusPoolReservationStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive bonus pool reservation in status requested.</summary>
    public static AdaptiveBonusPoolReservation NewAdaptiveBonusPoolReservation(string code, string label)
    {
        return new AdaptiveBonusPoolReservation(
            code,
            label,
            "",
            "",
            0.0,
            0,
            0,
            "",
            "",
            false,
            0,
            AdaptiveBonusPoolReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBonusPoolReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBonusPoolReservationStatus next)
    {
        switch (Status)
        {
            case AdaptiveBonusPoolReservationStatus.Requested:
                return next == AdaptiveBonusPoolReservationStatus.Revoked || next == AdaptiveBonusPoolReservationStatus.Expired;
            case AdaptiveBonusPoolReservationStatus.Granted:
                switch (next)
                {
                    case AdaptiveBonusPoolReservationStatus.Revoked:
                    case AdaptiveBonusPoolReservationStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveBonusPoolReservationStatus.Revoked:
                return next == AdaptiveBonusPoolReservationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptiveBonusPoolReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive bonus pool reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive bonus pool reservation"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Threshold - (double)WindowDays, 2);
    }

    public AdaptiveBonusPoolReservation Copy()
    {
        return new AdaptiveBonusPoolReservation(
            Code,
            Label,
            Unit,
            Locale,
            Threshold,
            Capacity,
            WindowDays,
            Badge,
            Skill,
            Archived,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
