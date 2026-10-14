namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global statement of work target is in its life cycle.</summary>
public enum GlobalStatementOfWorkTargetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One global statement of work target, with what the planners need to act on it.</summary>
public sealed class GlobalStatementOfWorkTarget(string code, string name, double maximum, double rate, string externalRef, string jobCode, double floorValue, int batchSize, string notes, bool pinned, GlobalStatementOfWorkTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public double Rate { get; set; } = rate;
    public string ExternalRef { get; set; } = externalRef;
    public string JobCode { get; set; } = jobCode;
    public double FloorValue { get; set; } = floorValue;
    public int BatchSize { get; set; } = batchSize;
    public string Notes { get; set; } = notes;
    public bool Pinned { get; set; } = pinned;
    public GlobalStatementOfWorkTargetStatus Status { get; set; } = status;

    /// <summary>Starts a global statement of work target in status new.</summary>
    public static GlobalStatementOfWorkTarget NewGlobalStatementOfWorkTarget(string code, string name)
    {
        return new GlobalStatementOfWorkTarget(
            code,
            name,
            0.0,
            0.0,
            "",
            "",
            0.0,
            0,
            "",
            false,
            GlobalStatementOfWorkTargetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalStatementOfWorkTargetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalStatementOfWorkTargetStatus next)
    {
        switch (Status)
        {
            case GlobalStatementOfWorkTargetStatus.New:
                switch (next)
                {
                    case GlobalStatementOfWorkTargetStatus.Done:
                    case GlobalStatementOfWorkTargetStatus.Failed:
                        return true;
                }
                return false;
            case GlobalStatementOfWorkTargetStatus.Queued:
                switch (next)
                {
                    case GlobalStatementOfWorkTargetStatus.Done:
                    case GlobalStatementOfWorkTargetStatus.Failed:
                        return true;
                }
                return false;
            case GlobalStatementOfWorkTargetStatus.Running:
                return next == GlobalStatementOfWorkTargetStatus.Failed;
            case GlobalStatementOfWorkTargetStatus.Done:
                return next == GlobalStatementOfWorkTargetStatus.Failed;
        }
        return false;
    }

    public void MoveTo(GlobalStatementOfWorkTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global statement of work target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global statement of work target"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(FloorValue * Rate, 2);
    }

    public GlobalStatementOfWorkTarget Copy()
    {
        return new GlobalStatementOfWorkTarget(
            Code,
            Name,
            Maximum,
            Rate,
            ExternalRef,
            JobCode,
            FloorValue,
            BatchSize,
            Notes,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
