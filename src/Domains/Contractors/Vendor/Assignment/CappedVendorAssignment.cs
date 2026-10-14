namespace Acme.Workforce.Contractors.Vendor.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped vendor assignment is in its life cycle.</summary>
public enum CappedVendorAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A capped vendor assignment. Its code is unique within a tenant.</summary>
public sealed class CappedVendorAssignment(string code, string label, int revision, int batchSize, string channel, string checksum, string phone, string employeeId, CappedVendorAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int BatchSize { get; set; } = batchSize;
    public string Channel { get; set; } = channel;
    public string Checksum { get; set; } = checksum;
    public string Phone { get; set; } = phone;
    public string EmployeeId { get; set; } = employeeId;
    public CappedVendorAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped vendor assignment in status proposed.</summary>
    public static CappedVendorAssignment NewCappedVendorAssignment(string code, string label)
    {
        return new CappedVendorAssignment(code, label, 0, 0, "", "", "", "", CappedVendorAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedVendorAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedVendorAssignmentStatus next)
    {
        switch (Status)
        {
            case CappedVendorAssignmentStatus.Proposed:
                switch (next)
                {
                    case CappedVendorAssignmentStatus.Approved:
                    case CappedVendorAssignmentStatus.Rejected:
                        return true;
                }
                return false;
            case CappedVendorAssignmentStatus.Approved:
                switch (next)
                {
                    case CappedVendorAssignmentStatus.Rejected:
                        return true;
                }
                return false;
            case CappedVendorAssignmentStatus.Rejected:
                switch (next)
                {
                    case CappedVendorAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedVendorAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped vendor assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Revision * (double)BatchSize, 2);
    }

    public CappedVendorAssignment Copy()
    {
        return new CappedVendorAssignment(
            Code,
            Label,
            Revision,
            BatchSize,
            Channel,
            Checksum,
            Phone,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
