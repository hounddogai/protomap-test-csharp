namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return audit is in its life cycle.</summary>
public enum AssetReturnAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A asset return audit. Its code is unique within a tenant.</summary>
public sealed class AssetReturnAudit(string code, string name, string costCenter, string skill, double floorValue, bool visible, int leaveDays, string source, string summary, string jobCode, string phone, AssetReturnAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public string Skill { get; set; } = skill;
    public double FloorValue { get; set; } = floorValue;
    public bool Visible { get; set; } = visible;
    public int LeaveDays { get; set; } = leaveDays;
    public string Source { get; set; } = source;
    public string Summary { get; set; } = summary;
    public string JobCode { get; set; } = jobCode;
    public string Phone { get; set; } = phone;
    public AssetReturnAuditStatus Status { get; set; } = status;

    /// <summary>Starts a asset return audit in status proposed.</summary>
    public static AssetReturnAudit NewAssetReturnAudit(string code, string name)
    {
        return new AssetReturnAudit(
            code,
            name,
            "",
            "",
            0.0,
            false,
            0,
            "",
            "",
            "",
            "",
            AssetReturnAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnAuditStatus next)
    {
        switch (Status)
        {
            case AssetReturnAuditStatus.Proposed:
                switch (next)
                {
                    case AssetReturnAuditStatus.Approved:
                    case AssetReturnAuditStatus.Withdrawn:
                        return true;
                }
                return false;
            case AssetReturnAuditStatus.Approved:
                return next == AssetReturnAuditStatus.Rejected;
            case AssetReturnAuditStatus.Rejected:
                switch (next)
                {
                    case AssetReturnAuditStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round(FloorValue + (double)LeaveDays, 2);
    }

    public AssetReturnAudit Copy()
    {
        return new AssetReturnAudit(
            Code,
            Name,
            CostCenter,
            Skill,
            FloorValue,
            Visible,
            LeaveDays,
            Source,
            Summary,
            JobCode,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
