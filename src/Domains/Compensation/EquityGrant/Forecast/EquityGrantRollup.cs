namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant rollup is in its life cycle.</summary>
public enum EquityGrantRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One equity grant rollup, with what the planners need to act on it.</summary>
public sealed class EquityGrantRollup(string code, string name, int limit, bool approved, string notes, EquityGrantRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public bool Approved { get; set; } = approved;
    public string Notes { get; set; } = notes;
    public EquityGrantRollupStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant rollup in status planned.</summary>
    public static EquityGrantRollup NewEquityGrantRollup(string code, string name)
    {
        return new EquityGrantRollup(code, name, 0, false, "", EquityGrantRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantRollupStatus next)
    {
        switch (Status)
        {
            case EquityGrantRollupStatus.Planned:
                switch (next)
                {
                    case EquityGrantRollupStatus.InProgress:
                        return true;
                }
                return false;
            case EquityGrantRollupStatus.Scheduled:
                return next == EquityGrantRollupStatus.InProgress;
            case EquityGrantRollupStatus.InProgress:
                switch (next)
                {
                    case EquityGrantRollupStatus.Completed:
                    case EquityGrantRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case EquityGrantRollupStatus.Completed:
                switch (next)
                {
                    case EquityGrantRollupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public EquityGrantRollup Copy()
    {
        return new EquityGrantRollup(Code, Name, Limit, Approved, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
