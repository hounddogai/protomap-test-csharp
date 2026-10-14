namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg checkpoint is in its life cycle.</summary>
public enum ReorgCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A reorg checkpoint as the reorg team keeps it.</summary>
public sealed class ReorgCheckpoint(string code, string name, double hours, string skill, bool approved, string source, ReorgCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public string Skill { get; set; } = skill;
    public bool Approved { get; set; } = approved;
    public string Source { get; set; } = source;
    public ReorgCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a reorg checkpoint in status draft.</summary>
    public static ReorgCheckpoint NewReorgCheckpoint(string code, string name)
    {
        return new ReorgCheckpoint(code, name, 0.0, "", false, "", ReorgCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgCheckpointStatus next)
    {
        switch (Status)
        {
            case ReorgCheckpointStatus.Draft:
                return next == ReorgCheckpointStatus.Suspended || next == ReorgCheckpointStatus.Closed;
            case ReorgCheckpointStatus.Pending:
                switch (next)
                {
                    case ReorgCheckpointStatus.Suspended:
                        return true;
                }
                return false;
            case ReorgCheckpointStatus.Active:
                switch (next)
                {
                    case ReorgCheckpointStatus.Closed:
                        return true;
                }
                return false;
            case ReorgCheckpointStatus.Suspended:
                switch (next)
                {
                    case ReorgCheckpointStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"reorg checkpoint"} {Code}";
    }

    public double MeasuredValue()
    {
        return Hours;
    }

    public ReorgCheckpoint Copy()
    {
        return new ReorgCheckpoint(Code, Name, Hours, Skill, Approved, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
