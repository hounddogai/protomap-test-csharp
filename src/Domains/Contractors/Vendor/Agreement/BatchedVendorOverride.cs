namespace Acme.Workforce.Contractors.Vendor.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched vendor override is in its life cycle.</summary>
public enum BatchedVendorOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched vendor override. Its code is unique within a tenant.</summary>
public sealed class BatchedVendorOverride(string code, string name, double rate, int tenureMonths, double amount, string currency, double rating, string timezone, string title, int quantity, string externalRef, BatchedVendorOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Amount { get; set; } = amount;
    public string Currency { get; set; } = currency;
    public double Rating { get; set; } = rating;
    public string Timezone { get; set; } = timezone;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string ExternalRef { get; set; } = externalRef;
    public BatchedVendorOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a batched vendor override in status proposed.</summary>
    public static BatchedVendorOverride NewBatchedVendorOverride(string code, string name)
    {
        return new BatchedVendorOverride(
            code,
            name,
            0.0,
            0,
            0.0,
            "",
            0.0,
            "",
            "",
            0,
            "",
            BatchedVendorOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedVendorOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedVendorOverrideStatus next)
    {
        switch (Status)
        {
            case BatchedVendorOverrideStatus.Proposed:
                return next == BatchedVendorOverrideStatus.Approved || next == BatchedVendorOverrideStatus.Rejected;
            case BatchedVendorOverrideStatus.Approved:
                switch (next)
                {
                    case BatchedVendorOverrideStatus.Rejected:
                    case BatchedVendorOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedVendorOverrideStatus.Rejected:
                return next == BatchedVendorOverrideStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BatchedVendorOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched vendor override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Amount + Rating, 2);
    }

    public BatchedVendorOverride Copy()
    {
        return new BatchedVendorOverride(
            Code,
            Name,
            Rate,
            TenureMonths,
            Amount,
            Currency,
            Rating,
            Timezone,
            Title,
            Quantity,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
