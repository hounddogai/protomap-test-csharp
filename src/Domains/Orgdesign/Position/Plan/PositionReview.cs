namespace Acme.Workforce.Orgdesign.Position.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position review is in its life cycle.</summary>
public enum PositionReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A position review. Its code is unique within a tenant.</summary>
public sealed class PositionReview(string code, string name, int retries, bool pinned, string channel, int capacity, double discount, int windowDays, string contactEmail, int batchSize, string department, PositionReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public bool Pinned { get; set; } = pinned;
    public string Channel { get; set; } = channel;
    public int Capacity { get; set; } = capacity;
    public double Discount { get; set; } = discount;
    public int WindowDays { get; set; } = windowDays;
    public string ContactEmail { get; set; } = contactEmail;
    public int BatchSize { get; set; } = batchSize;
    public string Department { get; set; } = department;
    public PositionReviewStatus Status { get; set; } = status;

    /// <summary>Starts a position review in status open.</summary>
    public static PositionReview NewPositionReview(string code, string name)
    {
        return new PositionReview(code, name, 0, false, "", 0, 0.0, 0, "", 0, "", PositionReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionReviewStatus next)
    {
        switch (Status)
        {
            case PositionReviewStatus.Open:
                return next == PositionReviewStatus.Resolved;
            case PositionReviewStatus.InReview:
                return next == PositionReviewStatus.Resolved || next == PositionReviewStatus.Archived;
            case PositionReviewStatus.Resolved:
                return next == PositionReviewStatus.Reopened || next == PositionReviewStatus.Archived;
            case PositionReviewStatus.Reopened:
                switch (next)
                {
                    case PositionReviewStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)BatchSize * (double)WindowDays, 2);
    }

    public PositionReview Copy()
    {
        return new PositionReview(
            Code,
            Name,
            Retries,
            Pinned,
            Channel,
            Capacity,
            Discount,
            WindowDays,
            ContactEmail,
            BatchSize,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
