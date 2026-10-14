namespace Acme.Workforce.Contractors.Vendor.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor baseline is in its life cycle.</summary>
public enum VendorBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A vendor baseline. Its code is unique within a tenant.</summary>
public sealed class VendorBaseline(string code, string title, double fte, string employeeId, List<string> tags, string category, VendorBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public string EmployeeId { get; set; } = employeeId;
    public List<string> Tags { get; set; } = tags;
    public string Category { get; set; } = category;
    public VendorBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a vendor baseline in status open.</summary>
    public static VendorBaseline NewVendorBaseline(string code, string title)
    {
        return new VendorBaseline(code, title, 0.0, "", new List<string>(), "", VendorBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorBaselineStatus next)
    {
        switch (Status)
        {
            case VendorBaselineStatus.Open:
                return next == VendorBaselineStatus.Resolved || next == VendorBaselineStatus.Reopened;
            case VendorBaselineStatus.InReview:
                switch (next)
                {
                    case VendorBaselineStatus.Archived:
                        return true;
                }
                return false;
            case VendorBaselineStatus.Resolved:
                switch (next)
                {
                    case VendorBaselineStatus.Reopened:
                    case VendorBaselineStatus.Archived:
                        return true;
                }
                return false;
            case VendorBaselineStatus.Reopened:
                return next == VendorBaselineStatus.Archived;
        }
        return false;
    }

    public void MoveTo(VendorBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double MeasuredValue()
    {
        return Fte;
    }

    public VendorBaseline Copy()
    {
        return new VendorBaseline(Code, Title, Fte, EmployeeId, Tags, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
