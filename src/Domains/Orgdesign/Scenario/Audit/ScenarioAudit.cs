namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario audit is in its life cycle.</summary>
public enum ScenarioAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One scenario audit, with what the planners need to act on it.</summary>
public sealed class ScenarioAudit(string code, string title, int revision, int batchSize, double floorValue, string category, string phone, int sequence, int attempts, bool archived, ScenarioAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public int BatchSize { get; set; } = batchSize;
    public double FloorValue { get; set; } = floorValue;
    public string Category { get; set; } = category;
    public string Phone { get; set; } = phone;
    public int Sequence { get; set; } = sequence;
    public int Attempts { get; set; } = attempts;
    public bool Archived { get; set; } = archived;
    public ScenarioAuditStatus Status { get; set; } = status;

    /// <summary>Starts a scenario audit in status planned.</summary>
    public static ScenarioAudit NewScenarioAudit(string code, string title)
    {
        return new ScenarioAudit(code, title, 0, 0, 0.0, "", "", 0, 0, false, ScenarioAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioAuditStatus next)
    {
        switch (Status)
        {
            case ScenarioAuditStatus.Planned:
                switch (next)
                {
                    case ScenarioAuditStatus.Scheduled:
                    case ScenarioAuditStatus.InProgress:
                        return true;
                }
                return false;
            case ScenarioAuditStatus.Scheduled:
                return next == ScenarioAuditStatus.InProgress || next == ScenarioAuditStatus.Cancelled;
            case ScenarioAuditStatus.InProgress:
                return next == ScenarioAuditStatus.Completed || next == ScenarioAuditStatus.Cancelled;
            case ScenarioAuditStatus.Completed:
                switch (next)
                {
                    case ScenarioAuditStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)BatchSize + FloorValue, 2);
    }

    public ScenarioAudit Copy()
    {
        return new ScenarioAudit(
            Code,
            Title,
            Revision,
            BatchSize,
            FloorValue,
            Category,
            Phone,
            Sequence,
            Attempts,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
