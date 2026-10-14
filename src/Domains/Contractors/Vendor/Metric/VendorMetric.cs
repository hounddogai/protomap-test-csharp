namespace Acme.Workforce.Contractors.Vendor.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor metric is in its life cycle.</summary>
public enum VendorMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A vendor metric. Its code is unique within a tenant.</summary>
public sealed class VendorMetric(string code, string label, string location, double maximum, double amount, string jobCode, bool archived, VendorMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public double Maximum { get; set; } = maximum;
    public double Amount { get; set; } = amount;
    public string JobCode { get; set; } = jobCode;
    public bool Archived { get; set; } = archived;
    public VendorMetricStatus Status { get; set; } = status;

    /// <summary>Starts a vendor metric in status requested.</summary>
    public static VendorMetric NewVendorMetric(string code, string label)
    {
        return new VendorMetric(code, label, "", 0.0, 0.0, "", false, VendorMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorMetricStatus next)
    {
        switch (Status)
        {
            case VendorMetricStatus.Requested:
                switch (next)
                {
                    case VendorMetricStatus.Revoked:
                    case VendorMetricStatus.Expired:
                        return true;
                }
                return false;
            case VendorMetricStatus.Granted:
                switch (next)
                {
                    case VendorMetricStatus.Revoked:
                    case VendorMetricStatus.Expired:
                        return true;
                }
                return false;
            case VendorMetricStatus.Revoked:
                return next == VendorMetricStatus.Expired;
        }
        return false;
    }

    public void MoveTo(VendorMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedLoad()
    {
        return Math.Round(Maximum * Amount, 2);
    }

    public VendorMetric Copy()
    {
        return new VendorMetric(Code, Label, Location, Maximum, Amount, JobCode, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
