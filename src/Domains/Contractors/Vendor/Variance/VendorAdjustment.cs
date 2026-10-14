namespace Acme.Workforce.Contractors.Vendor.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor adjustment is in its life cycle.</summary>
public enum VendorAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A vendor adjustment. Its code is unique within a tenant.</summary>
public sealed class VendorAdjustment(string code, string title, string grade, string team, string location, int createdAt, VendorAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public string Team { get; set; } = team;
    public string Location { get; set; } = location;
    public int CreatedAt { get; set; } = createdAt;
    public VendorAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a vendor adjustment in status idle.</summary>
    public static VendorAdjustment NewVendorAdjustment(string code, string title)
    {
        return new VendorAdjustment(code, title, "", "", "", 0, VendorAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorAdjustmentStatus next)
    {
        switch (Status)
        {
            case VendorAdjustmentStatus.Idle:
                switch (next)
                {
                    case VendorAdjustmentStatus.Ready:
                        return true;
                }
                return false;
            case VendorAdjustmentStatus.Warming:
                switch (next)
                {
                    case VendorAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case VendorAdjustmentStatus.Ready:
                switch (next)
                {
                    case VendorAdjustmentStatus.Draining:
                    case VendorAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case VendorAdjustmentStatus.Draining:
                switch (next)
                {
                    case VendorAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public VendorAdjustment Copy()
    {
        return new VendorAdjustment(Code, Title, Grade, Team, Location, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
