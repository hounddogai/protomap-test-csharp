namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped equity grant target is in its life cycle.</summary>
public enum CappedEquityGrantTargetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped equity grant target. Its code is unique within a tenant.</summary>
public sealed class CappedEquityGrantTarget(string code, string name, double rate, string title, string employeeId, CappedEquityGrantTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public CappedEquityGrantTargetStatus Status { get; set; } = status;

    /// <summary>Starts a capped equity grant target in status new.</summary>
    public static CappedEquityGrantTarget NewCappedEquityGrantTarget(string code, string name)
    {
        return new CappedEquityGrantTarget(code, name, 0.0, "", "", CappedEquityGrantTargetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedEquityGrantTargetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedEquityGrantTargetStatus next)
    {
        switch (Status)
        {
            case CappedEquityGrantTargetStatus.New:
                switch (next)
                {
                    case CappedEquityGrantTargetStatus.Queued:
                    case CappedEquityGrantTargetStatus.Running:
                        return true;
                }
                return false;
            case CappedEquityGrantTargetStatus.Queued:
                switch (next)
                {
                    case CappedEquityGrantTargetStatus.Done:
                    case CappedEquityGrantTargetStatus.Failed:
                        return true;
                }
                return false;
            case CappedEquityGrantTargetStatus.Running:
                switch (next)
                {
                    case CappedEquityGrantTargetStatus.Done:
                    case CappedEquityGrantTargetStatus.Failed:
                        return true;
                }
                return false;
            case CappedEquityGrantTargetStatus.Done:
                switch (next)
                {
                    case CappedEquityGrantTargetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedEquityGrantTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped equity grant target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public CappedEquityGrantTarget Copy()
    {
        return new CappedEquityGrantTarget(Code, Name, Rate, Title, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
