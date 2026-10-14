namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped asset return assignment is in its life cycle.</summary>
public enum CappedAssetReturnAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A capped asset return assignment as the asset return team keeps it.</summary>
public sealed class CappedAssetReturnAssignment(string code, string label, bool approved, double score, string costCenter, string timezone, string badge, int updatedAt, bool visible, string skill, CappedAssetReturnAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public double Score { get; set; } = score;
    public string CostCenter { get; set; } = costCenter;
    public string Timezone { get; set; } = timezone;
    public string Badge { get; set; } = badge;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Visible { get; set; } = visible;
    public string Skill { get; set; } = skill;
    public CappedAssetReturnAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped asset return assignment in status idle.</summary>
    public static CappedAssetReturnAssignment NewCappedAssetReturnAssignment(string code, string label)
    {
        return new CappedAssetReturnAssignment(
            code,
            label,
            false,
            0.0,
            "",
            "",
            "",
            0,
            false,
            "",
            CappedAssetReturnAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAssetReturnAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAssetReturnAssignmentStatus next)
    {
        switch (Status)
        {
            case CappedAssetReturnAssignmentStatus.Idle:
                return next == CappedAssetReturnAssignmentStatus.Draining || next == CappedAssetReturnAssignmentStatus.Stopped;
            case CappedAssetReturnAssignmentStatus.Warming:
                return next == CappedAssetReturnAssignmentStatus.Ready;
            case CappedAssetReturnAssignmentStatus.Ready:
                switch (next)
                {
                    case CappedAssetReturnAssignmentStatus.Draining:
                    case CappedAssetReturnAssignmentStatus.Stopped:
                        return true;
                }
                return false;
            case CappedAssetReturnAssignmentStatus.Draining:
                return next == CappedAssetReturnAssignmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CappedAssetReturnAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped asset return assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"capped asset return assignment"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Score * (double)UpdatedAt, 2);
    }

    public CappedAssetReturnAssignment Copy()
    {
        return new CappedAssetReturnAssignment(
            Code,
            Label,
            Approved,
            Score,
            CostCenter,
            Timezone,
            Badge,
            UpdatedAt,
            Visible,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
