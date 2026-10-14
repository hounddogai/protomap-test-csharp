namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination estimate is in its life cycle.</summary>
public enum NominationEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A nomination estimate. Its code is unique within a tenant.</summary>
public sealed class NominationEstimate(string code, string label, bool visible, string department, string phone, string summary, string costCenter, int quantity, string managerId, double ratio, int sequence, NominationEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public string Department { get; set; } = department;
    public string Phone { get; set; } = phone;
    public string Summary { get; set; } = summary;
    public string CostCenter { get; set; } = costCenter;
    public int Quantity { get; set; } = quantity;
    public string ManagerId { get; set; } = managerId;
    public double Ratio { get; set; } = ratio;
    public int Sequence { get; set; } = sequence;
    public NominationEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a nomination estimate in status proposed.</summary>
    public static NominationEstimate NewNominationEstimate(string code, string label)
    {
        return new NominationEstimate(
            code,
            label,
            false,
            "",
            "",
            "",
            "",
            0,
            "",
            0.0,
            0,
            NominationEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationEstimateStatus next)
    {
        switch (Status)
        {
            case NominationEstimateStatus.Proposed:
                return next == NominationEstimateStatus.Approved || next == NominationEstimateStatus.Withdrawn;
            case NominationEstimateStatus.Approved:
                switch (next)
                {
                    case NominationEstimateStatus.Rejected:
                    case NominationEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
            case NominationEstimateStatus.Rejected:
                switch (next)
                {
                    case NominationEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Quantity - Ratio, 2);
    }

    public NominationEstimate Copy()
    {
        return new NominationEstimate(
            Code,
            Label,
            Visible,
            Department,
            Phone,
            Summary,
            CostCenter,
            Quantity,
            ManagerId,
            Ratio,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
