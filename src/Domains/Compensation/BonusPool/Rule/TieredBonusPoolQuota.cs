namespace Acme.Workforce.Compensation.BonusPool.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered bonus pool quota is in its life cycle.</summary>
public enum TieredBonusPoolQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A tiered bonus pool quota as the bonus pool team keeps it.</summary>
public sealed class TieredBonusPoolQuota(string code, string name, double margin, string summary, double weight, int leaveDays, string contactEmail, string category, double rating, TieredBonusPoolQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public string Summary { get; set; } = summary;
    public double Weight { get; set; } = weight;
    public int LeaveDays { get; set; } = leaveDays;
    public string ContactEmail { get; set; } = contactEmail;
    public string Category { get; set; } = category;
    public double Rating { get; set; } = rating;
    public TieredBonusPoolQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a tiered bonus pool quota in status open.</summary>
    public static TieredBonusPoolQuota NewTieredBonusPoolQuota(string code, string name)
    {
        return new TieredBonusPoolQuota(code, name, 0.0, "", 0.0, 0, "", "", 0.0, TieredBonusPoolQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredBonusPoolQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredBonusPoolQuotaStatus next)
    {
        switch (Status)
        {
            case TieredBonusPoolQuotaStatus.Open:
                return next == TieredBonusPoolQuotaStatus.Resolved || next == TieredBonusPoolQuotaStatus.Archived;
            case TieredBonusPoolQuotaStatus.InReview:
                switch (next)
                {
                    case TieredBonusPoolQuotaStatus.Reopened:
                    case TieredBonusPoolQuotaStatus.Archived:
                        return true;
                }
                return false;
            case TieredBonusPoolQuotaStatus.Resolved:
                switch (next)
                {
                    case TieredBonusPoolQuotaStatus.Archived:
                        return true;
                }
                return false;
            case TieredBonusPoolQuotaStatus.Reopened:
                switch (next)
                {
                    case TieredBonusPoolQuotaStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredBonusPoolQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered bonus pool quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Weight + Rating, 2);
    }

    public TieredBonusPoolQuota Copy()
    {
        return new TieredBonusPoolQuota(
            Code,
            Name,
            Margin,
            Summary,
            Weight,
            LeaveDays,
            ContactEmail,
            Category,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
