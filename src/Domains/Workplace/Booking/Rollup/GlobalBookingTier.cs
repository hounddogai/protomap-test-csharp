namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global booking tier is in its life cycle.</summary>
public enum GlobalBookingTierStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A global booking tier as the booking team keeps it.</summary>
public sealed class GlobalBookingTier(string code, string title, string notes, int limit, string badge, double discount, string grade, int priority, string skill, double rate, int windowDays, GlobalBookingTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public int Limit { get; set; } = limit;
    public string Badge { get; set; } = badge;
    public double Discount { get; set; } = discount;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public string Skill { get; set; } = skill;
    public double Rate { get; set; } = rate;
    public int WindowDays { get; set; } = windowDays;
    public GlobalBookingTierStatus Status { get; set; } = status;

    /// <summary>Starts a global booking tier in status requested.</summary>
    public static GlobalBookingTier NewGlobalBookingTier(string code, string title)
    {
        return new GlobalBookingTier(
            code,
            title,
            "",
            0,
            "",
            0.0,
            "",
            0,
            "",
            0.0,
            0,
            GlobalBookingTierStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalBookingTierStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalBookingTierStatus next)
    {
        switch (Status)
        {
            case GlobalBookingTierStatus.Requested:
                return next == GlobalBookingTierStatus.Granted || next == GlobalBookingTierStatus.Revoked;
            case GlobalBookingTierStatus.Granted:
                switch (next)
                {
                    case GlobalBookingTierStatus.Revoked:
                    case GlobalBookingTierStatus.Expired:
                        return true;
                }
                return false;
            case GlobalBookingTierStatus.Revoked:
                switch (next)
                {
                    case GlobalBookingTierStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalBookingTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global booking tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Limit - (double)WindowDays, 2);
    }

    public GlobalBookingTier Copy()
    {
        return new GlobalBookingTier(
            Code,
            Title,
            Notes,
            Limit,
            Badge,
            Discount,
            Grade,
            Priority,
            Skill,
            Rate,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
