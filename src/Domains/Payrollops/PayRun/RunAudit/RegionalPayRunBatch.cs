namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional pay run batch is in its life cycle.</summary>
public enum RegionalPayRunBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a regional pay run batch and its current status.</summary>
public sealed class RegionalPayRunBatch(string code, string name, string skill, string country, int version, RegionalPayRunBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public string Country { get; set; } = country;
    public int Version { get; set; } = version;
    public RegionalPayRunBatchStatus Status { get; set; } = status;

    /// <summary>Starts a regional pay run batch in status planned.</summary>
    public static RegionalPayRunBatch NewRegionalPayRunBatch(string code, string name)
    {
        return new RegionalPayRunBatch(code, name, "", "", 0, RegionalPayRunBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPayRunBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPayRunBatchStatus next)
    {
        switch (Status)
        {
            case RegionalPayRunBatchStatus.Planned:
                return next == RegionalPayRunBatchStatus.InProgress;
            case RegionalPayRunBatchStatus.Scheduled:
                return next == RegionalPayRunBatchStatus.InProgress || next == RegionalPayRunBatchStatus.Completed;
            case RegionalPayRunBatchStatus.InProgress:
                return next == RegionalPayRunBatchStatus.Cancelled;
            case RegionalPayRunBatchStatus.Completed:
                switch (next)
                {
                    case RegionalPayRunBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalPayRunBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional pay run batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public RegionalPayRunBatch Copy()
    {
        return new RegionalPayRunBatch(Code, Name, Skill, Country, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
