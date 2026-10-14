namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment score is in its life cycle.</summary>
public enum AdjustmentScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adjustment score as the adjustment team keeps it.</summary>
public sealed class AdjustmentScore(string code, string title, string department, string source, int startsAt, string channel, string currency, bool visible, string managerId, string label, AdjustmentScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public string Source { get; set; } = source;
    public int StartsAt { get; set; } = startsAt;
    public string Channel { get; set; } = channel;
    public string Currency { get; set; } = currency;
    public bool Visible { get; set; } = visible;
    public string ManagerId { get; set; } = managerId;
    public string Label { get; set; } = label;
    public AdjustmentScoreStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment score in status open.</summary>
    public static AdjustmentScore NewAdjustmentScore(string code, string title)
    {
        return new AdjustmentScore(code, title, "", "", 0, "", "", false, "", "", AdjustmentScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentScoreStatus next)
    {
        switch (Status)
        {
            case AdjustmentScoreStatus.Open:
                return next == AdjustmentScoreStatus.InReview;
            case AdjustmentScoreStatus.InReview:
                switch (next)
                {
                    case AdjustmentScoreStatus.Reopened:
                    case AdjustmentScoreStatus.Archived:
                        return true;
                }
                return false;
            case AdjustmentScoreStatus.Resolved:
                return next == AdjustmentScoreStatus.Reopened;
            case AdjustmentScoreStatus.Reopened:
                return next == AdjustmentScoreStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdjustmentScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public AdjustmentScore Copy()
    {
        return new AdjustmentScore(
            Code,
            Title,
            Department,
            Source,
            StartsAt,
            Channel,
            Currency,
            Visible,
            ManagerId,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
