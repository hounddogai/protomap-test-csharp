namespace Acme.Workforce.Payrollops.Garnishment.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered garnishment quota is in its life cycle.</summary>
public enum TieredGarnishmentQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A tiered garnishment quota as the garnishment team keeps it.</summary>
public sealed class TieredGarnishmentQuota(string code, string title, int createdAt, double maximum, string name, string region, TieredGarnishmentQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public double Maximum { get; set; } = maximum;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public TieredGarnishmentQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a tiered garnishment quota in status open.</summary>
    public static TieredGarnishmentQuota NewTieredGarnishmentQuota(string code, string title)
    {
        return new TieredGarnishmentQuota(code, title, 0, 0.0, "", "", TieredGarnishmentQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredGarnishmentQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredGarnishmentQuotaStatus next)
    {
        switch (Status)
        {
            case TieredGarnishmentQuotaStatus.Open:
                switch (next)
                {
                    case TieredGarnishmentQuotaStatus.Reopened:
                    case TieredGarnishmentQuotaStatus.Archived:
                        return true;
                }
                return false;
            case TieredGarnishmentQuotaStatus.InReview:
                return next == TieredGarnishmentQuotaStatus.Reopened;
            case TieredGarnishmentQuotaStatus.Resolved:
                return next == TieredGarnishmentQuotaStatus.Reopened || next == TieredGarnishmentQuotaStatus.Archived;
            case TieredGarnishmentQuotaStatus.Reopened:
                return next == TieredGarnishmentQuotaStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TieredGarnishmentQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered garnishment quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Maximum - (double)CreatedAt, 2);
    }

    public TieredGarnishmentQuota Copy()
    {
        return new TieredGarnishmentQuota(Code, Title, CreatedAt, Maximum, Name, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
