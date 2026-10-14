namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered leave request item is in its life cycle.</summary>
public enum TieredLeaveRequestItemStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered leave request item. Its code is unique within a tenant.</summary>
public sealed class TieredLeaveRequestItem(string code, string title, int durationMinutes, string team, string locale, string contactEmail, double minimum, double cost, string managerId, int startsAt, TieredLeaveRequestItemStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Team { get; set; } = team;
    public string Locale { get; set; } = locale;
    public string ContactEmail { get; set; } = contactEmail;
    public double Minimum { get; set; } = minimum;
    public double Cost { get; set; } = cost;
    public string ManagerId { get; set; } = managerId;
    public int StartsAt { get; set; } = startsAt;
    public TieredLeaveRequestItemStatus Status { get; set; } = status;

    /// <summary>Starts a tiered leave request item in status requested.</summary>
    public static TieredLeaveRequestItem NewTieredLeaveRequestItem(string code, string title)
    {
        return new TieredLeaveRequestItem(
            code,
            title,
            0,
            "",
            "",
            "",
            0.0,
            0.0,
            "",
            0,
            TieredLeaveRequestItemStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredLeaveRequestItemStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredLeaveRequestItemStatus next)
    {
        switch (Status)
        {
            case TieredLeaveRequestItemStatus.Requested:
                return next == TieredLeaveRequestItemStatus.Expired;
            case TieredLeaveRequestItemStatus.Granted:
                switch (next)
                {
                    case TieredLeaveRequestItemStatus.Revoked:
                    case TieredLeaveRequestItemStatus.Expired:
                        return true;
                }
                return false;
            case TieredLeaveRequestItemStatus.Revoked:
                switch (next)
                {
                    case TieredLeaveRequestItemStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredLeaveRequestItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered leave request item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)DurationMinutes + Cost, 2);
    }

    public TieredLeaveRequestItem Copy()
    {
        return new TieredLeaveRequestItem(
            Code,
            Title,
            DurationMinutes,
            Team,
            Locale,
            ContactEmail,
            Minimum,
            Cost,
            ManagerId,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
