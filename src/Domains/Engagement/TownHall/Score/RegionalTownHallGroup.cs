namespace Acme.Workforce.Engagement.TownHall.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional town hall group is in its life cycle.</summary>
public enum RegionalTownHallGroupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A regional town hall group as the town hall team keeps it.</summary>
public sealed class RegionalTownHallGroup(string code, string label, string name, string badge, int quantity, int durationMinutes, string ownerId, string skill, double fte, RegionalTownHallGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public int Quantity { get; set; } = quantity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string OwnerId { get; set; } = ownerId;
    public string Skill { get; set; } = skill;
    public double Fte { get; set; } = fte;
    public RegionalTownHallGroupStatus Status { get; set; } = status;

    /// <summary>Starts a regional town hall group in status proposed.</summary>
    public static RegionalTownHallGroup NewRegionalTownHallGroup(string code, string label)
    {
        return new RegionalTownHallGroup(
            code,
            label,
            "",
            "",
            0,
            0,
            "",
            "",
            0.0,
            RegionalTownHallGroupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTownHallGroupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTownHallGroupStatus next)
    {
        switch (Status)
        {
            case RegionalTownHallGroupStatus.Proposed:
                return next == RegionalTownHallGroupStatus.Approved || next == RegionalTownHallGroupStatus.Withdrawn;
            case RegionalTownHallGroupStatus.Approved:
                return next == RegionalTownHallGroupStatus.Rejected || next == RegionalTownHallGroupStatus.Withdrawn;
            case RegionalTownHallGroupStatus.Rejected:
                return next == RegionalTownHallGroupStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RegionalTownHallGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional town hall group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Fte - (double)DurationMinutes, 2);
    }

    public RegionalTownHallGroup Copy()
    {
        return new RegionalTownHallGroup(
            Code,
            Label,
            Name,
            Badge,
            Quantity,
            DurationMinutes,
            OwnerId,
            Skill,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
