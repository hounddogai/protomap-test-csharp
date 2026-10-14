namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global vendor review is in its life cycle.</summary>
public enum GlobalVendorReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global vendor review. Its code is unique within a tenant.</summary>
public sealed class GlobalVendorReview(string code, string name, int batchSize, string channel, string country, string region, int tenureMonths, int priority, string externalRef, string ownerId, double ceiling, GlobalVendorReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public string Channel { get; set; } = channel;
    public string Country { get; set; } = country;
    public string Region { get; set; } = region;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Priority { get; set; } = priority;
    public string ExternalRef { get; set; } = externalRef;
    public string OwnerId { get; set; } = ownerId;
    public double Ceiling { get; set; } = ceiling;
    public GlobalVendorReviewStatus Status { get; set; } = status;

    /// <summary>Starts a global vendor review in status planned.</summary>
    public static GlobalVendorReview NewGlobalVendorReview(string code, string name)
    {
        return new GlobalVendorReview(
            code,
            name,
            0,
            "",
            "",
            "",
            0,
            0,
            "",
            "",
            0.0,
            GlobalVendorReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalVendorReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalVendorReviewStatus next)
    {
        switch (Status)
        {
            case GlobalVendorReviewStatus.Planned:
                switch (next)
                {
                    case GlobalVendorReviewStatus.Scheduled:
                    case GlobalVendorReviewStatus.InProgress:
                        return true;
                }
                return false;
            case GlobalVendorReviewStatus.Scheduled:
                return next == GlobalVendorReviewStatus.InProgress || next == GlobalVendorReviewStatus.Cancelled;
            case GlobalVendorReviewStatus.InProgress:
                return next == GlobalVendorReviewStatus.Completed || next == GlobalVendorReviewStatus.Cancelled;
            case GlobalVendorReviewStatus.Completed:
                return next == GlobalVendorReviewStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GlobalVendorReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global vendor review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)BatchSize * (double)Priority, 2);
    }

    public GlobalVendorReview Copy()
    {
        return new GlobalVendorReview(
            Code,
            Name,
            BatchSize,
            Channel,
            Country,
            Region,
            TenureMonths,
            Priority,
            ExternalRef,
            OwnerId,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
