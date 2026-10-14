namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched scenario audit is in its life cycle.</summary>
public enum BatchedScenarioAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched scenario audit as the scenario team keeps it.</summary>
public sealed class BatchedScenarioAudit(string code, string label, bool pinned, double fte, int version, int batchSize, bool active, int graceDays, int retries, string skill, BatchedScenarioAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public double Fte { get; set; } = fte;
    public int Version { get; set; } = version;
    public int BatchSize { get; set; } = batchSize;
    public bool Active { get; set; } = active;
    public int GraceDays { get; set; } = graceDays;
    public int Retries { get; set; } = retries;
    public string Skill { get; set; } = skill;
    public BatchedScenarioAuditStatus Status { get; set; } = status;

    /// <summary>Starts a batched scenario audit in status proposed.</summary>
    public static BatchedScenarioAudit NewBatchedScenarioAudit(string code, string label)
    {
        return new BatchedScenarioAudit(
            code,
            label,
            false,
            0.0,
            0,
            0,
            false,
            0,
            0,
            "",
            BatchedScenarioAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedScenarioAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedScenarioAuditStatus next)
    {
        switch (Status)
        {
            case BatchedScenarioAuditStatus.Proposed:
                switch (next)
                {
                    case BatchedScenarioAuditStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedScenarioAuditStatus.Approved:
                switch (next)
                {
                    case BatchedScenarioAuditStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedScenarioAuditStatus.Rejected:
                switch (next)
                {
                    case BatchedScenarioAuditStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedScenarioAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched scenario audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)GraceDays - Fte, 2);
    }

    public BatchedScenarioAudit Copy()
    {
        return new BatchedScenarioAudit(
            Code,
            Label,
            Pinned,
            Fte,
            Version,
            BatchSize,
            Active,
            GraceDays,
            Retries,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
