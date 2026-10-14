namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy town hall checkpoint is in its life cycle.</summary>
public enum LegacyTownHallCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A legacy town hall checkpoint as the town hall team keeps it.</summary>
public sealed class LegacyTownHallCheckpoint(string code, string label, double fte, bool archived, string region, string country, string name, string externalRef, double threshold, double hours, LegacyTownHallCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public bool Archived { get; set; } = archived;
    public string Region { get; set; } = region;
    public string Country { get; set; } = country;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public double Threshold { get; set; } = threshold;
    public double Hours { get; set; } = hours;
    public LegacyTownHallCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a legacy town hall checkpoint in status new.</summary>
    public static LegacyTownHallCheckpoint NewLegacyTownHallCheckpoint(string code, string label)
    {
        return new LegacyTownHallCheckpoint(
            code,
            label,
            0.0,
            false,
            "",
            "",
            "",
            "",
            0.0,
            0.0,
            LegacyTownHallCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTownHallCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTownHallCheckpointStatus next)
    {
        switch (Status)
        {
            case LegacyTownHallCheckpointStatus.New:
                return next == LegacyTownHallCheckpointStatus.Queued || next == LegacyTownHallCheckpointStatus.Done;
            case LegacyTownHallCheckpointStatus.Queued:
                switch (next)
                {
                    case LegacyTownHallCheckpointStatus.Running:
                    case LegacyTownHallCheckpointStatus.Failed:
                        return true;
                }
                return false;
            case LegacyTownHallCheckpointStatus.Running:
                return next == LegacyTownHallCheckpointStatus.Done || next == LegacyTownHallCheckpointStatus.Failed;
            case LegacyTownHallCheckpointStatus.Done:
                return next == LegacyTownHallCheckpointStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyTownHallCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy town hall checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedVolume()
    {
        return Math.Round(Fte - Hours, 2);
    }

    public LegacyTownHallCheckpoint Copy()
    {
        return new LegacyTownHallCheckpoint(
            Code,
            Label,
            Fte,
            Archived,
            Region,
            Country,
            Name,
            ExternalRef,
            Threshold,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
