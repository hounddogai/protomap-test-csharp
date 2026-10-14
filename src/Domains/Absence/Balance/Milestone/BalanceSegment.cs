namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance segment is in its life cycle.</summary>
public enum BalanceSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One balance segment, with what the planners need to act on it.</summary>
public sealed class BalanceSegment(string code, string title, int updatedAt, int createdAt, double budget, double weight, double discount, BalanceSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public int CreatedAt { get; set; } = createdAt;
    public double Budget { get; set; } = budget;
    public double Weight { get; set; } = weight;
    public double Discount { get; set; } = discount;
    public BalanceSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a balance segment in status draft.</summary>
    public static BalanceSegment NewBalanceSegment(string code, string title)
    {
        return new BalanceSegment(code, title, 0, 0, 0.0, 0.0, 0.0, BalanceSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceSegmentStatus next)
    {
        switch (Status)
        {
            case BalanceSegmentStatus.Draft:
                switch (next)
                {
                    case BalanceSegmentStatus.Active:
                        return true;
                }
                return false;
            case BalanceSegmentStatus.Pending:
                switch (next)
                {
                    case BalanceSegmentStatus.Suspended:
                        return true;
                }
                return false;
            case BalanceSegmentStatus.Active:
                return next == BalanceSegmentStatus.Suspended || next == BalanceSegmentStatus.Closed;
            case BalanceSegmentStatus.Suspended:
                return next == BalanceSegmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BalanceSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Weight - Budget, 2);
    }

    public BalanceSegment Copy()
    {
        return new BalanceSegment(Code, Title, UpdatedAt, CreatedAt, Budget, Weight, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
