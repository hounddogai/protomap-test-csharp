namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy review is in its life cycle.</summary>
public enum OccupancyReviewStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a occupancy review and its current status.</summary>
public sealed class OccupancyReview(string code, string label, int quantity, int startsAt, int capacity, string ownerId, int leaveDays, string category, OccupancyReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public int StartsAt { get; set; } = startsAt;
    public int Capacity { get; set; } = capacity;
    public string OwnerId { get; set; } = ownerId;
    public int LeaveDays { get; set; } = leaveDays;
    public string Category { get; set; } = category;
    public OccupancyReviewStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy review in status requested.</summary>
    public static OccupancyReview NewOccupancyReview(string code, string label)
    {
        return new OccupancyReview(code, label, 0, 0, 0, "", 0, "", OccupancyReviewStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyReviewStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyReviewStatus next)
    {
        switch (Status)
        {
            case OccupancyReviewStatus.Requested:
                return next == OccupancyReviewStatus.Revoked;
            case OccupancyReviewStatus.Granted:
                switch (next)
                {
                    case OccupancyReviewStatus.Revoked:
                    case OccupancyReviewStatus.Expired:
                        return true;
                }
                return false;
            case OccupancyReviewStatus.Revoked:
                switch (next)
                {
                    case OccupancyReviewStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Quantity + (double)StartsAt, 2);
    }

    public OccupancyReview Copy()
    {
        return new OccupancyReview(Code, Label, Quantity, StartsAt, Capacity, OwnerId, LeaveDays, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
