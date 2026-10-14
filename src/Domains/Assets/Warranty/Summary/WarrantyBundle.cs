namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty bundle is in its life cycle.</summary>
public enum WarrantyBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A warranty bundle. Its code is unique within a tenant.</summary>
public sealed class WarrantyBundle(string code, string title, string unit, int expiresAt, string badge, int endsAt, int updatedAt, string description, int leaveDays, int batchSize, string phone, WarrantyBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Badge { get; set; } = badge;
    public int EndsAt { get; set; } = endsAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Description { get; set; } = description;
    public int LeaveDays { get; set; } = leaveDays;
    public int BatchSize { get; set; } = batchSize;
    public string Phone { get; set; } = phone;
    public WarrantyBundleStatus Status { get; set; } = status;

    /// <summary>Starts a warranty bundle in status requested.</summary>
    public static WarrantyBundle NewWarrantyBundle(string code, string title)
    {
        return new WarrantyBundle(code, title, "", 0, "", 0, 0, "", 0, 0, "", WarrantyBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyBundleStatus next)
    {
        switch (Status)
        {
            case WarrantyBundleStatus.Requested:
                return next == WarrantyBundleStatus.Granted || next == WarrantyBundleStatus.Expired;
            case WarrantyBundleStatus.Granted:
                switch (next)
                {
                    case WarrantyBundleStatus.Revoked:
                    case WarrantyBundleStatus.Expired:
                        return true;
                }
                return false;
            case WarrantyBundleStatus.Revoked:
                return next == WarrantyBundleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(WarrantyBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)ExpiresAt + (double)UpdatedAt, 2);
    }

    public WarrantyBundle Copy()
    {
        return new WarrantyBundle(
            Code,
            Title,
            Unit,
            ExpiresAt,
            Badge,
            EndsAt,
            UpdatedAt,
            Description,
            LeaveDays,
            BatchSize,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
