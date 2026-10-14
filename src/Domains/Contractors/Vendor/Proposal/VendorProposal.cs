namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor proposal is in its life cycle.</summary>
public enum VendorProposalStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A vendor proposal. Its code is unique within a tenant.</summary>
public sealed class VendorProposal(string code, string label, string costCenter, int capacity, string grade, string description, int startsAt, bool visible, string employeeId, int batchSize, VendorProposalStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public int Capacity { get; set; } = capacity;
    public string Grade { get; set; } = grade;
    public string Description { get; set; } = description;
    public int StartsAt { get; set; } = startsAt;
    public bool Visible { get; set; } = visible;
    public string EmployeeId { get; set; } = employeeId;
    public int BatchSize { get; set; } = batchSize;
    public VendorProposalStatus Status { get; set; } = status;

    /// <summary>Starts a vendor proposal in status draft.</summary>
    public static VendorProposal NewVendorProposal(string code, string label)
    {
        return new VendorProposal(code, label, "", 0, "", "", 0, false, "", 0, VendorProposalStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorProposalStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorProposalStatus next)
    {
        switch (Status)
        {
            case VendorProposalStatus.Draft:
                switch (next)
                {
                    case VendorProposalStatus.Pending:
                    case VendorProposalStatus.Active:
                        return true;
                }
                return false;
            case VendorProposalStatus.Pending:
                return next == VendorProposalStatus.Active || next == VendorProposalStatus.Suspended;
            case VendorProposalStatus.Active:
                switch (next)
                {
                    case VendorProposalStatus.Suspended:
                    case VendorProposalStatus.Closed:
                        return true;
                }
                return false;
            case VendorProposalStatus.Suspended:
                return next == VendorProposalStatus.Closed;
        }
        return false;
    }

    public void MoveTo(VendorProposalStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor proposal {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)StartsAt + (double)Capacity, 2);
    }

    public VendorProposal Copy()
    {
        return new VendorProposal(
            Code,
            Label,
            CostCenter,
            Capacity,
            Grade,
            Description,
            StartsAt,
            Visible,
            EmployeeId,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
