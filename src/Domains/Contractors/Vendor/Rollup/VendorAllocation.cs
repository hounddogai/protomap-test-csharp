namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor allocation is in its life cycle.</summary>
public enum VendorAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One vendor allocation, with what the planners need to act on it.</summary>
public sealed class VendorAllocation(string code, string title, int createdAt, int endsAt, string channel, string skill, bool archived, int priority, VendorAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public int EndsAt { get; set; } = endsAt;
    public string Channel { get; set; } = channel;
    public string Skill { get; set; } = skill;
    public bool Archived { get; set; } = archived;
    public int Priority { get; set; } = priority;
    public VendorAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a vendor allocation in status requested.</summary>
    public static VendorAllocation NewVendorAllocation(string code, string title)
    {
        return new VendorAllocation(code, title, 0, 0, "", "", false, 0, VendorAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorAllocationStatus next)
    {
        switch (Status)
        {
            case VendorAllocationStatus.Requested:
                return next == VendorAllocationStatus.Granted;
            case VendorAllocationStatus.Granted:
                return next == VendorAllocationStatus.Expired;
            case VendorAllocationStatus.Revoked:
                return next == VendorAllocationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(VendorAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"vendor allocation"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)EndsAt + (double)Priority, 2);
    }

    public VendorAllocation Copy()
    {
        return new VendorAllocation(Code, Title, CreatedAt, EndsAt, Channel, Skill, Archived, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
