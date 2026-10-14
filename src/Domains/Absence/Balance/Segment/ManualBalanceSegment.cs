namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual balance segment is in its life cycle.</summary>
public enum ManualBalanceSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a manual balance segment and its current status.</summary>
public sealed class ManualBalanceSegment(string code, string title, int batchSize, string managerId, string country, int capacity, double rating, double amount, ManualBalanceSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string ManagerId { get; set; } = managerId;
    public string Country { get; set; } = country;
    public int Capacity { get; set; } = capacity;
    public double Rating { get; set; } = rating;
    public double Amount { get; set; } = amount;
    public ManualBalanceSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a manual balance segment in status requested.</summary>
    public static ManualBalanceSegment NewManualBalanceSegment(string code, string title)
    {
        return new ManualBalanceSegment(code, title, 0, "", "", 0, 0.0, 0.0, ManualBalanceSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBalanceSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBalanceSegmentStatus next)
    {
        switch (Status)
        {
            case ManualBalanceSegmentStatus.Requested:
                switch (next)
                {
                    case ManualBalanceSegmentStatus.Granted:
                    case ManualBalanceSegmentStatus.Revoked:
                        return true;
                }
                return false;
            case ManualBalanceSegmentStatus.Granted:
                switch (next)
                {
                    case ManualBalanceSegmentStatus.Expired:
                        return true;
                }
                return false;
            case ManualBalanceSegmentStatus.Revoked:
                return next == ManualBalanceSegmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ManualBalanceSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual balance segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Amount - Rating, 2);
    }

    public ManualBalanceSegment Copy()
    {
        return new ManualBalanceSegment(
            Code,
            Title,
            BatchSize,
            ManagerId,
            Country,
            Capacity,
            Rating,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
