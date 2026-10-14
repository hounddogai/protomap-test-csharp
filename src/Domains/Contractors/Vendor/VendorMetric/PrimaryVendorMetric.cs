namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary vendor metric is in its life cycle.</summary>
public enum PrimaryVendorMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a primary vendor metric and its current status.</summary>
public sealed class PrimaryVendorMetric(string code, string label, int revision, int retries, string department, string summary, PrimaryVendorMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int Retries { get; set; } = retries;
    public string Department { get; set; } = department;
    public string Summary { get; set; } = summary;
    public PrimaryVendorMetricStatus Status { get; set; } = status;

    /// <summary>Starts a primary vendor metric in status open.</summary>
    public static PrimaryVendorMetric NewPrimaryVendorMetric(string code, string label)
    {
        return new PrimaryVendorMetric(code, label, 0, 0, "", "", PrimaryVendorMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryVendorMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryVendorMetricStatus next)
    {
        switch (Status)
        {
            case PrimaryVendorMetricStatus.Open:
                return next == PrimaryVendorMetricStatus.InReview || next == PrimaryVendorMetricStatus.Resolved;
            case PrimaryVendorMetricStatus.InReview:
                return next == PrimaryVendorMetricStatus.Archived;
            case PrimaryVendorMetricStatus.Resolved:
                switch (next)
                {
                    case PrimaryVendorMetricStatus.Reopened:
                    case PrimaryVendorMetricStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryVendorMetricStatus.Reopened:
                switch (next)
                {
                    case PrimaryVendorMetricStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryVendorMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary vendor metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"primary vendor metric"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Revision * (double)Retries, 2);
    }

    public PrimaryVendorMetric Copy()
    {
        return new PrimaryVendorMetric(Code, Label, Revision, Retries, Department, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
