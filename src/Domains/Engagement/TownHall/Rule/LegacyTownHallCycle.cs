namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy town hall cycle is in its life cycle.</summary>
public enum LegacyTownHallCycleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy town hall cycle. Its code is unique within a tenant.</summary>
public sealed class LegacyTownHallCycle(string code, string title, int quantity, List<string> tags, bool approved, string summary, string managerId, LegacyTownHallCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public List<string> Tags { get; set; } = tags;
    public bool Approved { get; set; } = approved;
    public string Summary { get; set; } = summary;
    public string ManagerId { get; set; } = managerId;
    public LegacyTownHallCycleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy town hall cycle in status open.</summary>
    public static LegacyTownHallCycle NewLegacyTownHallCycle(string code, string title)
    {
        return new LegacyTownHallCycle(
            code,
            title,
            0,
            new List<string>(),
            false,
            "",
            "",
            LegacyTownHallCycleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTownHallCycleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTownHallCycleStatus next)
    {
        switch (Status)
        {
            case LegacyTownHallCycleStatus.Open:
                return next == LegacyTownHallCycleStatus.InReview;
            case LegacyTownHallCycleStatus.InReview:
                switch (next)
                {
                    case LegacyTownHallCycleStatus.Resolved:
                        return true;
                }
                return false;
            case LegacyTownHallCycleStatus.Resolved:
                return next == LegacyTownHallCycleStatus.Reopened || next == LegacyTownHallCycleStatus.Archived;
            case LegacyTownHallCycleStatus.Reopened:
                switch (next)
                {
                    case LegacyTownHallCycleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyTownHallCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy town hall cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public LegacyTownHallCycle Copy()
    {
        return new LegacyTownHallCycle(Code, Title, Quantity, Tags, Approved, Summary, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
