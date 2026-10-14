namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor milestone is in its life cycle.</summary>
public enum VendorMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A vendor milestone. Its code is unique within a tenant.</summary>
public sealed class VendorMilestone(string code, string title, string region, int quantity, string badge, double rate, int updatedAt, double score, double amount, VendorMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public int Quantity { get; set; } = quantity;
    public string Badge { get; set; } = badge;
    public double Rate { get; set; } = rate;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Score { get; set; } = score;
    public double Amount { get; set; } = amount;
    public VendorMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a vendor milestone in status planned.</summary>
    public static VendorMilestone NewVendorMilestone(string code, string title)
    {
        return new VendorMilestone(code, title, "", 0, "", 0.0, 0, 0.0, 0.0, VendorMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorMilestoneStatus next)
    {
        switch (Status)
        {
            case VendorMilestoneStatus.Planned:
                switch (next)
                {
                    case VendorMilestoneStatus.Completed:
                    case VendorMilestoneStatus.Cancelled:
                        return true;
                }
                return false;
            case VendorMilestoneStatus.Scheduled:
                return next == VendorMilestoneStatus.InProgress || next == VendorMilestoneStatus.Completed;
            case VendorMilestoneStatus.InProgress:
                return next == VendorMilestoneStatus.Completed;
            case VendorMilestoneStatus.Completed:
                return next == VendorMilestoneStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(VendorMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"vendor milestone"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Quantity + Amount, 2);
    }

    public VendorMilestone Copy()
    {
        return new VendorMilestone(Code, Title, Region, Quantity, Badge, Rate, UpdatedAt, Score, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
