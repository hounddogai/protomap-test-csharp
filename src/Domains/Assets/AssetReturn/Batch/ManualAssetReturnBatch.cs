namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual asset return batch is in its life cycle.</summary>
public enum ManualAssetReturnBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One manual asset return batch, with what the planners need to act on it.</summary>
public sealed class ManualAssetReturnBatch(string code, string label, int priority, string costCenter, string employeeId, double rate, int leaveDays, ManualAssetReturnBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string CostCenter { get; set; } = costCenter;
    public string EmployeeId { get; set; } = employeeId;
    public double Rate { get; set; } = rate;
    public int LeaveDays { get; set; } = leaveDays;
    public ManualAssetReturnBatchStatus Status { get; set; } = status;

    /// <summary>Starts a manual asset return batch in status proposed.</summary>
    public static ManualAssetReturnBatch NewManualAssetReturnBatch(string code, string label)
    {
        return new ManualAssetReturnBatch(code, label, 0, "", "", 0.0, 0, ManualAssetReturnBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAssetReturnBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAssetReturnBatchStatus next)
    {
        switch (Status)
        {
            case ManualAssetReturnBatchStatus.Proposed:
                return next == ManualAssetReturnBatchStatus.Approved || next == ManualAssetReturnBatchStatus.Withdrawn;
            case ManualAssetReturnBatchStatus.Approved:
                switch (next)
                {
                    case ManualAssetReturnBatchStatus.Rejected:
                        return true;
                }
                return false;
            case ManualAssetReturnBatchStatus.Rejected:
                return next == ManualAssetReturnBatchStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualAssetReturnBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual asset return batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Priority * (double)LeaveDays, 2);
    }

    public ManualAssetReturnBatch Copy()
    {
        return new ManualAssetReturnBatch(Code, Label, Priority, CostCenter, EmployeeId, Rate, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
