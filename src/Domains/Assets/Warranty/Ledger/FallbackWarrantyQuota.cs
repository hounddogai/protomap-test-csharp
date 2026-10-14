namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback warranty quota is in its life cycle.</summary>
public enum FallbackWarrantyQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a fallback warranty quota and its current status.</summary>
public sealed class FallbackWarrantyQuota(string code, string title, string source, string region, bool archived, int startsAt, FallbackWarrantyQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Region { get; set; } = region;
    public bool Archived { get; set; } = archived;
    public int StartsAt { get; set; } = startsAt;
    public FallbackWarrantyQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a fallback warranty quota in status created.</summary>
    public static FallbackWarrantyQuota NewFallbackWarrantyQuota(string code, string title)
    {
        return new FallbackWarrantyQuota(code, title, "", "", false, 0, FallbackWarrantyQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackWarrantyQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackWarrantyQuotaStatus next)
    {
        switch (Status)
        {
            case FallbackWarrantyQuotaStatus.Created:
                switch (next)
                {
                    case FallbackWarrantyQuotaStatus.Published:
                    case FallbackWarrantyQuotaStatus.Retired:
                        return true;
                }
                return false;
            case FallbackWarrantyQuotaStatus.Validated:
                switch (next)
                {
                    case FallbackWarrantyQuotaStatus.Published:
                        return true;
                }
                return false;
            case FallbackWarrantyQuotaStatus.Published:
                switch (next)
                {
                    case FallbackWarrantyQuotaStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackWarrantyQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback warranty quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public FallbackWarrantyQuota Copy()
    {
        return new FallbackWarrantyQuota(Code, Title, Source, Region, Archived, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
