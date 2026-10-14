namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending town hall baseline is in its life cycle.</summary>
public enum PendingTownHallBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending town hall baseline. Its code is unique within a tenant.</summary>
public sealed class PendingTownHallBaseline(string code, string title, int priority, int sequence, string channel, string team, string location, double margin, double discount, PendingTownHallBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public int Sequence { get; set; } = sequence;
    public string Channel { get; set; } = channel;
    public string Team { get; set; } = team;
    public string Location { get; set; } = location;
    public double Margin { get; set; } = margin;
    public double Discount { get; set; } = discount;
    public PendingTownHallBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a pending town hall baseline in status proposed.</summary>
    public static PendingTownHallBaseline NewPendingTownHallBaseline(string code, string title)
    {
        return new PendingTownHallBaseline(
            code,
            title,
            0,
            0,
            "",
            "",
            "",
            0.0,
            0.0,
            PendingTownHallBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingTownHallBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingTownHallBaselineStatus next)
    {
        switch (Status)
        {
            case PendingTownHallBaselineStatus.Proposed:
                switch (next)
                {
                    case PendingTownHallBaselineStatus.Approved:
                        return true;
                }
                return false;
            case PendingTownHallBaselineStatus.Approved:
                switch (next)
                {
                    case PendingTownHallBaselineStatus.Rejected:
                        return true;
                }
                return false;
            case PendingTownHallBaselineStatus.Rejected:
                switch (next)
                {
                    case PendingTownHallBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingTownHallBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending town hall baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetTotal()
    {
        return Math.Round(Discount + (double)Sequence, 2);
    }

    public PendingTownHallBaseline Copy()
    {
        return new PendingTownHallBaseline(
            Code,
            Title,
            Priority,
            Sequence,
            Channel,
            Team,
            Location,
            Margin,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
