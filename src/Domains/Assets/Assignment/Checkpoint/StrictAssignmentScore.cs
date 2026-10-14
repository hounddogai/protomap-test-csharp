namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict assignment score is in its life cycle.</summary>
public enum StrictAssignmentScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a strict assignment score and its current status.</summary>
public sealed class StrictAssignmentScore(string code, string name, string notes, bool approved, double floorValue, int revision, string ownerId, string title, int version, string location, StrictAssignmentScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public bool Approved { get; set; } = approved;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public string OwnerId { get; set; } = ownerId;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public string Location { get; set; } = location;
    public StrictAssignmentScoreStatus Status { get; set; } = status;

    /// <summary>Starts a strict assignment score in status new.</summary>
    public static StrictAssignmentScore NewStrictAssignmentScore(string code, string name)
    {
        return new StrictAssignmentScore(
            code,
            name,
            "",
            false,
            0.0,
            0,
            "",
            "",
            0,
            "",
            StrictAssignmentScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAssignmentScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAssignmentScoreStatus next)
    {
        switch (Status)
        {
            case StrictAssignmentScoreStatus.New:
                return next == StrictAssignmentScoreStatus.Queued;
            case StrictAssignmentScoreStatus.Queued:
                return next == StrictAssignmentScoreStatus.Running;
            case StrictAssignmentScoreStatus.Running:
                return next == StrictAssignmentScoreStatus.Failed;
            case StrictAssignmentScoreStatus.Done:
                return next == StrictAssignmentScoreStatus.Failed;
        }
        return false;
    }

    public void MoveTo(StrictAssignmentScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict assignment score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Version - FloorValue, 2);
    }

    public StrictAssignmentScore Copy()
    {
        return new StrictAssignmentScore(
            Code,
            Name,
            Notes,
            Approved,
            FloorValue,
            Revision,
            OwnerId,
            Title,
            Version,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
