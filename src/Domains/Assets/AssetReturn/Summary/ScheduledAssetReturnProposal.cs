namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled asset return proposal is in its life cycle.</summary>
public enum ScheduledAssetReturnProposalStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A scheduled asset return proposal as the asset return team keeps it.</summary>
public sealed class ScheduledAssetReturnProposal(string code, string name, string employeeId, string department, int version, bool visible, string grade, string region, bool approved, ScheduledAssetReturnProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public string Department { get; set; } = department;
    public int Version { get; set; } = version;
    public bool Visible { get; set; } = visible;
    public string Grade { get; set; } = grade;
    public string Region { get; set; } = region;
    public bool Approved { get; set; } = approved;
    public ScheduledAssetReturnProposalStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled asset return proposal in status requested.</summary>
    public static ScheduledAssetReturnProposal NewScheduledAssetReturnProposal(string code, string name)
    {
        return new ScheduledAssetReturnProposal(
            code,
            name,
            "",
            "",
            0,
            false,
            "",
            "",
            false,
            ScheduledAssetReturnProposalStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAssetReturnProposalStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAssetReturnProposalStatus next)
    {
        switch (Status)
        {
            case ScheduledAssetReturnProposalStatus.Requested:
                return next == ScheduledAssetReturnProposalStatus.Expired;
            case ScheduledAssetReturnProposalStatus.Granted:
                switch (next)
                {
                    case ScheduledAssetReturnProposalStatus.Expired:
                        return true;
                }
                return false;
            case ScheduledAssetReturnProposalStatus.Revoked:
                return next == ScheduledAssetReturnProposalStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ScheduledAssetReturnProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled asset return proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public ScheduledAssetReturnProposal Copy()
    {
        return new ScheduledAssetReturnProposal(
            Code,
            Name,
            EmployeeId,
            Department,
            Version,
            Visible,
            Grade,
            Region,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
