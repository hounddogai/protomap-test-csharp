namespace Acme.Workforce.Contractors.Vendor.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy vendor batch is in its life cycle.</summary>
public enum LegacyVendorBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A legacy vendor batch as the vendor team keeps it.</summary>
public sealed class LegacyVendorBatch(string code, string title, double rating, string label, bool active, string costCenter, double budget, string externalRef, int leaveDays, LegacyVendorBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public string CostCenter { get; set; } = costCenter;
    public double Budget { get; set; } = budget;
    public string ExternalRef { get; set; } = externalRef;
    public int LeaveDays { get; set; } = leaveDays;
    public LegacyVendorBatchStatus Status { get; set; } = status;

    /// <summary>Starts a legacy vendor batch in status new.</summary>
    public static LegacyVendorBatch NewLegacyVendorBatch(string code, string title)
    {
        return new LegacyVendorBatch(code, title, 0.0, "", false, "", 0.0, "", 0, LegacyVendorBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyVendorBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyVendorBatchStatus next)
    {
        switch (Status)
        {
            case LegacyVendorBatchStatus.New:
                return next == LegacyVendorBatchStatus.Done;
            case LegacyVendorBatchStatus.Queued:
                switch (next)
                {
                    case LegacyVendorBatchStatus.Running:
                        return true;
                }
                return false;
            case LegacyVendorBatchStatus.Running:
                switch (next)
                {
                    case LegacyVendorBatchStatus.Done:
                        return true;
                }
                return false;
            case LegacyVendorBatchStatus.Done:
                return next == LegacyVendorBatchStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyVendorBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy vendor batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double NetValue()
    {
        return Math.Round(Budget + Rating, 2);
    }

    public LegacyVendorBatch Copy()
    {
        return new LegacyVendorBatch(
            Code,
            Title,
            Rating,
            Label,
            Active,
            CostCenter,
            Budget,
            ExternalRef,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
