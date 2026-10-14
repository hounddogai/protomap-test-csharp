namespace Acme.Workforce.Contractors.Vendor.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor variance is in its life cycle.</summary>
public enum VendorVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A vendor variance. Its code is unique within a tenant.</summary>
public sealed class VendorVariance(string code, string title, int batchSize, string costCenter, string phone, string unit, string reviewer, VendorVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string CostCenter { get; set; } = costCenter;
    public string Phone { get; set; } = phone;
    public string Unit { get; set; } = unit;
    public string Reviewer { get; set; } = reviewer;
    public VendorVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a vendor variance in status created.</summary>
    public static VendorVariance NewVendorVariance(string code, string title)
    {
        return new VendorVariance(code, title, 0, "", "", "", "", VendorVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorVarianceStatus next)
    {
        switch (Status)
        {
            case VendorVarianceStatus.Created:
                return next == VendorVarianceStatus.Published || next == VendorVarianceStatus.Retired;
            case VendorVarianceStatus.Validated:
                switch (next)
                {
                    case VendorVarianceStatus.Published:
                    case VendorVarianceStatus.Retired:
                        return true;
                }
                return false;
            case VendorVarianceStatus.Published:
                return next == VendorVarianceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(VendorVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"vendor variance"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public VendorVariance Copy()
    {
        return new VendorVariance(Code, Title, BatchSize, CostCenter, Phone, Unit, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
