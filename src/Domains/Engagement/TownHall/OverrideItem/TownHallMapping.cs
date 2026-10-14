namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall mapping is in its life cycle.</summary>
public enum TownHallMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A town hall mapping as the town hall team keeps it.</summary>
public sealed class TownHallMapping(string code, string title, double fte, string ownerId, string notes, int attempts, int version, int graceDays, string summary, string phone, string region, TownHallMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public string OwnerId { get; set; } = ownerId;
    public string Notes { get; set; } = notes;
    public int Attempts { get; set; } = attempts;
    public int Version { get; set; } = version;
    public int GraceDays { get; set; } = graceDays;
    public string Summary { get; set; } = summary;
    public string Phone { get; set; } = phone;
    public string Region { get; set; } = region;
    public TownHallMappingStatus Status { get; set; } = status;

    /// <summary>Starts a town hall mapping in status new.</summary>
    public static TownHallMapping NewTownHallMapping(string code, string title)
    {
        return new TownHallMapping(code, title, 0.0, "", "", 0, 0, 0, "", "", "", TownHallMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallMappingStatus next)
    {
        switch (Status)
        {
            case TownHallMappingStatus.New:
                return next == TownHallMappingStatus.Done;
            case TownHallMappingStatus.Queued:
                switch (next)
                {
                    case TownHallMappingStatus.Done:
                        return true;
                }
                return false;
            case TownHallMappingStatus.Running:
                return next == TownHallMappingStatus.Done;
            case TownHallMappingStatus.Done:
                switch (next)
                {
                    case TownHallMappingStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Version * Fte, 2);
    }

    public TownHallMapping Copy()
    {
        return new TownHallMapping(
            Code,
            Title,
            Fte,
            OwnerId,
            Notes,
            Attempts,
            Version,
            GraceDays,
            Summary,
            Phone,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
