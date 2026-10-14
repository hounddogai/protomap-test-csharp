namespace Acme.Workforce.Contractors.Vendor.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor assignment is in its life cycle.</summary>
public enum VendorAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A vendor assignment. Its code is unique within a tenant.</summary>
public sealed class VendorAssignment(string code, string title, int expiresAt, List<string> tags, int quantity, int capacity, double ratio, int tenureMonths, int revision, string ownerId, VendorAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public List<string> Tags { get; set; } = tags;
    public int Quantity { get; set; } = quantity;
    public int Capacity { get; set; } = capacity;
    public double Ratio { get; set; } = ratio;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Revision { get; set; } = revision;
    public string OwnerId { get; set; } = ownerId;
    public VendorAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a vendor assignment in status proposed.</summary>
    public static VendorAssignment NewVendorAssignment(string code, string title)
    {
        return new VendorAssignment(
            code,
            title,
            0,
            new List<string>(),
            0,
            0,
            0.0,
            0,
            0,
            "",
            VendorAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorAssignmentStatus next)
    {
        switch (Status)
        {
            case VendorAssignmentStatus.Proposed:
                return next == VendorAssignmentStatus.Withdrawn;
            case VendorAssignmentStatus.Approved:
                switch (next)
                {
                    case VendorAssignmentStatus.Rejected:
                    case VendorAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case VendorAssignmentStatus.Rejected:
                return next == VendorAssignmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(VendorAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Capacity - (double)Quantity, 2);
    }

    public VendorAssignment Copy()
    {
        return new VendorAssignment(
            Code,
            Title,
            ExpiresAt,
            Tags,
            Quantity,
            Capacity,
            Ratio,
            TenureMonths,
            Revision,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
