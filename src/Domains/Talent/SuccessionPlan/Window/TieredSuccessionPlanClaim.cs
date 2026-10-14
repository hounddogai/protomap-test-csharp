namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered succession plan claim is in its life cycle.</summary>
public enum TieredSuccessionPlanClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One tiered succession plan claim, with what the planners need to act on it.</summary>
public sealed class TieredSuccessionPlanClaim(string code, string title, string phone, string category, int updatedAt, string ownerId, TieredSuccessionPlanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public string Category { get; set; } = category;
    public int UpdatedAt { get; set; } = updatedAt;
    public string OwnerId { get; set; } = ownerId;
    public TieredSuccessionPlanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a tiered succession plan claim in status open.</summary>
    public static TieredSuccessionPlanClaim NewTieredSuccessionPlanClaim(string code, string title)
    {
        return new TieredSuccessionPlanClaim(code, title, "", "", 0, "", TieredSuccessionPlanClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSuccessionPlanClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSuccessionPlanClaimStatus next)
    {
        switch (Status)
        {
            case TieredSuccessionPlanClaimStatus.Open:
                switch (next)
                {
                    case TieredSuccessionPlanClaimStatus.InReview:
                    case TieredSuccessionPlanClaimStatus.Archived:
                        return true;
                }
                return false;
            case TieredSuccessionPlanClaimStatus.InReview:
                return next == TieredSuccessionPlanClaimStatus.Resolved || next == TieredSuccessionPlanClaimStatus.Archived;
            case TieredSuccessionPlanClaimStatus.Resolved:
                switch (next)
                {
                    case TieredSuccessionPlanClaimStatus.Reopened:
                        return true;
                }
                return false;
            case TieredSuccessionPlanClaimStatus.Reopened:
                switch (next)
                {
                    case TieredSuccessionPlanClaimStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredSuccessionPlanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered succession plan claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public TieredSuccessionPlanClaim Copy()
    {
        return new TieredSuccessionPlanClaim(Code, Title, Phone, Category, UpdatedAt, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
