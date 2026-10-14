namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped garnishment review is in its life cycle.</summary>
public enum CappedGarnishmentReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A capped garnishment review as the garnishment team keeps it.</summary>
public sealed class CappedGarnishmentReview(string code, string title, string unit, int quantity, bool pinned, int attempts, double floorValue, int expiresAt, int createdAt, CappedGarnishmentReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public int Quantity { get; set; } = quantity;
    public bool Pinned { get; set; } = pinned;
    public int Attempts { get; set; } = attempts;
    public double FloorValue { get; set; } = floorValue;
    public int ExpiresAt { get; set; } = expiresAt;
    public int CreatedAt { get; set; } = createdAt;
    public CappedGarnishmentReviewStatus Status { get; set; } = status;

    /// <summary>Starts a capped garnishment review in status open.</summary>
    public static CappedGarnishmentReview NewCappedGarnishmentReview(string code, string title)
    {
        return new CappedGarnishmentReview(
            code,
            title,
            "",
            0,
            false,
            0,
            0.0,
            0,
            0,
            CappedGarnishmentReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedGarnishmentReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedGarnishmentReviewStatus next)
    {
        switch (Status)
        {
            case CappedGarnishmentReviewStatus.Open:
                return next == CappedGarnishmentReviewStatus.Reopened;
            case CappedGarnishmentReviewStatus.InReview:
                return next == CappedGarnishmentReviewStatus.Archived;
            case CappedGarnishmentReviewStatus.Resolved:
                return next == CappedGarnishmentReviewStatus.Reopened;
            case CappedGarnishmentReviewStatus.Reopened:
                return next == CappedGarnishmentReviewStatus.Archived;
        }
        return false;
    }

    public void MoveTo(CappedGarnishmentReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped garnishment review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)CreatedAt - FloorValue, 2);
    }

    public CappedGarnishmentReview Copy()
    {
        return new CappedGarnishmentReview(
            Code,
            Title,
            Unit,
            Quantity,
            Pinned,
            Attempts,
            FloorValue,
            ExpiresAt,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
