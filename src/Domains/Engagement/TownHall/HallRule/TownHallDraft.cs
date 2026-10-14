namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall draft is in its life cycle.</summary>
public enum TownHallDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A town hall draft as the town hall team keeps it.</summary>
public sealed class TownHallDraft(string code, string name, int revision, bool pinned, double cost, string badge, int version, List<string> tags, string title, bool visible, TownHallDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public bool Pinned { get; set; } = pinned;
    public double Cost { get; set; } = cost;
    public string Badge { get; set; } = badge;
    public int Version { get; set; } = version;
    public List<string> Tags { get; set; } = tags;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public TownHallDraftStatus Status { get; set; } = status;

    /// <summary>Starts a town hall draft in status planned.</summary>
    public static TownHallDraft NewTownHallDraft(string code, string name)
    {
        return new TownHallDraft(
            code,
            name,
            0,
            false,
            0.0,
            "",
            0,
            new List<string>(),
            "",
            false,
            TownHallDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallDraftStatus next)
    {
        switch (Status)
        {
            case TownHallDraftStatus.Planned:
                return next == TownHallDraftStatus.Scheduled;
            case TownHallDraftStatus.Scheduled:
                return next == TownHallDraftStatus.Cancelled;
            case TownHallDraftStatus.InProgress:
                switch (next)
                {
                    case TownHallDraftStatus.Cancelled:
                        return true;
                }
                return false;
            case TownHallDraftStatus.Completed:
                return next == TownHallDraftStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TownHallDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double EffectiveVolume()
    {
        return Math.Round(Cost + (double)Revision, 2);
    }

    public TownHallDraft Copy()
    {
        return new TownHallDraft(Code, Name, Revision, Pinned, Cost, Badge, Version, Tags, Title, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
