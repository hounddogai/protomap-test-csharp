namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pay run checkpoint is in its life cycle.</summary>
public enum CappedPayRunCheckpointStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One capped pay run checkpoint, with what the planners need to act on it.</summary>
public sealed class CappedPayRunCheckpoint(string code, string name, int revision, int version, string region, string title, string summary, double fte, CappedPayRunCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public int Version { get; set; } = version;
    public string Region { get; set; } = region;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public double Fte { get; set; } = fte;
    public CappedPayRunCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a capped pay run checkpoint in status created.</summary>
    public static CappedPayRunCheckpoint NewCappedPayRunCheckpoint(string code, string name)
    {
        return new CappedPayRunCheckpoint(code, name, 0, 0, "", "", "", 0.0, CappedPayRunCheckpointStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPayRunCheckpointStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPayRunCheckpointStatus next)
    {
        switch (Status)
        {
            case CappedPayRunCheckpointStatus.Created:
                return next == CappedPayRunCheckpointStatus.Published;
            case CappedPayRunCheckpointStatus.Validated:
                return next == CappedPayRunCheckpointStatus.Published;
            case CappedPayRunCheckpointStatus.Published:
                switch (next)
                {
                    case CappedPayRunCheckpointStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedPayRunCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pay run checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"capped pay run checkpoint"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Revision + Fte, 2);
    }

    public CappedPayRunCheckpoint Copy()
    {
        return new CappedPayRunCheckpoint(Code, Name, Revision, Version, Region, Title, Summary, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
