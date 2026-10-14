namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg rollup is in its life cycle.</summary>
public enum ReorgRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A reorg rollup as the reorg team keeps it.</summary>
public sealed class ReorgRollup(string code, string name, string summary, double hours, double ceiling, ReorgRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public double Hours { get; set; } = hours;
    public double Ceiling { get; set; } = ceiling;
    public ReorgRollupStatus Status { get; set; } = status;

    /// <summary>Starts a reorg rollup in status planned.</summary>
    public static ReorgRollup NewReorgRollup(string code, string name)
    {
        return new ReorgRollup(code, name, "", 0.0, 0.0, ReorgRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgRollupStatus next)
    {
        switch (Status)
        {
            case ReorgRollupStatus.Planned:
                switch (next)
                {
                    case ReorgRollupStatus.InProgress:
                        return true;
                }
                return false;
            case ReorgRollupStatus.Scheduled:
                return next == ReorgRollupStatus.Completed;
            case ReorgRollupStatus.InProgress:
                return next == ReorgRollupStatus.Completed;
            case ReorgRollupStatus.Completed:
                switch (next)
                {
                    case ReorgRollupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Hours + Ceiling, 2);
    }

    public ReorgRollup Copy()
    {
        return new ReorgRollup(Code, Name, Summary, Hours, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
