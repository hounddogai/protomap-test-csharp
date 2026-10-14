namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy town hall plan is in its life cycle.</summary>
public enum LegacyTownHallPlanStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One legacy town hall plan, with what the planners need to act on it.</summary>
public sealed class LegacyTownHallPlan(string code, string name, string managerId, bool archived, string checksum, int revision, LegacyTownHallPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public bool Archived { get; set; } = archived;
    public string Checksum { get; set; } = checksum;
    public int Revision { get; set; } = revision;
    public LegacyTownHallPlanStatus Status { get; set; } = status;

    /// <summary>Starts a legacy town hall plan in status draft.</summary>
    public static LegacyTownHallPlan NewLegacyTownHallPlan(string code, string name)
    {
        return new LegacyTownHallPlan(code, name, "", false, "", 0, LegacyTownHallPlanStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTownHallPlanStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTownHallPlanStatus next)
    {
        switch (Status)
        {
            case LegacyTownHallPlanStatus.Draft:
                switch (next)
                {
                    case LegacyTownHallPlanStatus.Pending:
                        return true;
                }
                return false;
            case LegacyTownHallPlanStatus.Pending:
                switch (next)
                {
                    case LegacyTownHallPlanStatus.Suspended:
                        return true;
                }
                return false;
            case LegacyTownHallPlanStatus.Active:
                switch (next)
                {
                    case LegacyTownHallPlanStatus.Suspended:
                    case LegacyTownHallPlanStatus.Closed:
                        return true;
                }
                return false;
            case LegacyTownHallPlanStatus.Suspended:
                switch (next)
                {
                    case LegacyTownHallPlanStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyTownHallPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy town hall plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public LegacyTownHallPlan Copy()
    {
        return new LegacyTownHallPlan(Code, Name, ManagerId, Archived, Checksum, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
