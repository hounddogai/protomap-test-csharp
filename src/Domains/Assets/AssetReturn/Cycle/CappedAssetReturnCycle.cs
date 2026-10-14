namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped asset return cycle is in its life cycle.</summary>
public enum CappedAssetReturnCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A capped asset return cycle as the asset return team keeps it.</summary>
public sealed class CappedAssetReturnCycle(string code, string name, string jobCode, string costCenter, string badge, string grade, double budget, double maximum, bool approved, string team, string managerId, CappedAssetReturnCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string CostCenter { get; set; } = costCenter;
    public string Badge { get; set; } = badge;
    public string Grade { get; set; } = grade;
    public double Budget { get; set; } = budget;
    public double Maximum { get; set; } = maximum;
    public bool Approved { get; set; } = approved;
    public string Team { get; set; } = team;
    public string ManagerId { get; set; } = managerId;
    public CappedAssetReturnCycleStatus Status { get; set; } = status;

    /// <summary>Starts a capped asset return cycle in status idle.</summary>
    public static CappedAssetReturnCycle NewCappedAssetReturnCycle(string code, string name)
    {
        return new CappedAssetReturnCycle(
            code,
            name,
            "",
            "",
            "",
            "",
            0.0,
            0.0,
            false,
            "",
            "",
            CappedAssetReturnCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAssetReturnCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAssetReturnCycleStatus next)
    {
        switch (Status)
        {
            case CappedAssetReturnCycleStatus.Idle:
                switch (next)
                {
                    case CappedAssetReturnCycleStatus.Stopped:
                        return true;
                }
                return false;
            case CappedAssetReturnCycleStatus.Warming:
                return next == CappedAssetReturnCycleStatus.Draining;
            case CappedAssetReturnCycleStatus.Ready:
                switch (next)
                {
                    case CappedAssetReturnCycleStatus.Draining:
                    case CappedAssetReturnCycleStatus.Stopped:
                        return true;
                }
                return false;
            case CappedAssetReturnCycleStatus.Draining:
                return next == CappedAssetReturnCycleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CappedAssetReturnCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped asset return cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetExposure()
    {
        return Math.Round(Maximum + Budget, 2);
    }

    public CappedAssetReturnCycle Copy()
    {
        return new CappedAssetReturnCycle(
            Code,
            Name,
            JobCode,
            CostCenter,
            Badge,
            Grade,
            Budget,
            Maximum,
            Approved,
            Team,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
