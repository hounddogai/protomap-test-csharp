namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination checkpoint is in its life cycle.</summary>
public enum NominationCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A nomination checkpoint as the nomination team keeps it.</summary>
public sealed class NominationCheckpoint(string code, string title, string category, string unit, int capacity, List<string> tags, NominationCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Unit { get; set; } = unit;
    public int Capacity { get; set; } = capacity;
    public List<string> Tags { get; set; } = tags;
    public NominationCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a nomination checkpoint in status draft.</summary>
    public static NominationCheckpoint NewNominationCheckpoint(string code, string title)
    {
        return new NominationCheckpoint(
            code,
            title,
            "",
            "",
            0,
            new List<string>(),
            NominationCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationCheckpointStatus next)
    {
        switch (Status)
        {
            case NominationCheckpointStatus.Draft:
                switch (next)
                {
                    case NominationCheckpointStatus.Closed:
                        return true;
                }
                return false;
            case NominationCheckpointStatus.Pending:
                return next == NominationCheckpointStatus.Suspended;
            case NominationCheckpointStatus.Active:
                return next == NominationCheckpointStatus.Closed;
            case NominationCheckpointStatus.Suspended:
                switch (next)
                {
                    case NominationCheckpointStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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
        return (double)Capacity;
    }

    public NominationCheckpoint Copy()
    {
        return new NominationCheckpoint(Code, Title, Category, Unit, Capacity, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
