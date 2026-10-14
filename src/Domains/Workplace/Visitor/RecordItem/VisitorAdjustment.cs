namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor adjustment is in its life cycle.</summary>
public enum VisitorAdjustmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A visitor adjustment. Its code is unique within a tenant.</summary>
public sealed class VisitorAdjustment(string code, string title, int attempts, int windowDays, double score, int quantity, string region, int leaveDays, string channel, string country, VisitorAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public int WindowDays { get; set; } = windowDays;
    public double Score { get; set; } = score;
    public int Quantity { get; set; } = quantity;
    public string Region { get; set; } = region;
    public int LeaveDays { get; set; } = leaveDays;
    public string Channel { get; set; } = channel;
    public string Country { get; set; } = country;
    public VisitorAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a visitor adjustment in status proposed.</summary>
    public static VisitorAdjustment NewVisitorAdjustment(string code, string title)
    {
        return new VisitorAdjustment(code, title, 0, 0, 0.0, 0, "", 0, "", "", VisitorAdjustmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorAdjustmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorAdjustmentStatus next)
    {
        switch (Status)
        {
            case VisitorAdjustmentStatus.Proposed:
                return next == VisitorAdjustmentStatus.Approved;
            case VisitorAdjustmentStatus.Approved:
                return next == VisitorAdjustmentStatus.Rejected;
            case VisitorAdjustmentStatus.Rejected:
                return next == VisitorAdjustmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(VisitorAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Quantity - Score, 2);
    }

    public VisitorAdjustment Copy()
    {
        return new VisitorAdjustment(
            Code,
            Title,
            Attempts,
            WindowDays,
            Score,
            Quantity,
            Region,
            LeaveDays,
            Channel,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
