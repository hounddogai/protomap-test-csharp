namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form entry is in its life cycle.</summary>
public enum TaxFormEntryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tax form entry as the tax form team keeps it.</summary>
public sealed class TaxFormEntry(string code, string label, int batchSize, int expiresAt, string employeeId, string region, string badge, string checksum, double cost, TaxFormEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public int ExpiresAt { get; set; } = expiresAt;
    public string EmployeeId { get; set; } = employeeId;
    public string Region { get; set; } = region;
    public string Badge { get; set; } = badge;
    public string Checksum { get; set; } = checksum;
    public double Cost { get; set; } = cost;
    public TaxFormEntryStatus Status { get; set; } = status;

    /// <summary>Starts a tax form entry in status requested.</summary>
    public static TaxFormEntry NewTaxFormEntry(string code, string label)
    {
        return new TaxFormEntry(code, label, 0, 0, "", "", "", "", 0.0, TaxFormEntryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormEntryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormEntryStatus next)
    {
        switch (Status)
        {
            case TaxFormEntryStatus.Requested:
                return next == TaxFormEntryStatus.Revoked || next == TaxFormEntryStatus.Expired;
            case TaxFormEntryStatus.Granted:
                return next == TaxFormEntryStatus.Revoked || next == TaxFormEntryStatus.Expired;
            case TaxFormEntryStatus.Revoked:
                switch (next)
                {
                    case TaxFormEntryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)BatchSize * Cost, 2);
    }

    public TaxFormEntry Copy()
    {
        return new TaxFormEntry(
            Code,
            Label,
            BatchSize,
            ExpiresAt,
            EmployeeId,
            Region,
            Badge,
            Checksum,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
