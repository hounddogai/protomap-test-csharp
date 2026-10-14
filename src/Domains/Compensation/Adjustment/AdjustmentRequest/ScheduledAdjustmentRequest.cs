namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled adjustment request is in its life cycle.</summary>
public enum ScheduledAdjustmentRequestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scheduled adjustment request. Its code is unique within a tenant.</summary>
public sealed class ScheduledAdjustmentRequest(string code, string label, string region, string location, int limit, string badge, int sequence, string ownerId, string channel, string team, ScheduledAdjustmentRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public string Location { get; set; } = location;
    public int Limit { get; set; } = limit;
    public string Badge { get; set; } = badge;
    public int Sequence { get; set; } = sequence;
    public string OwnerId { get; set; } = ownerId;
    public string Channel { get; set; } = channel;
    public string Team { get; set; } = team;
    public ScheduledAdjustmentRequestStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled adjustment request in status open.</summary>
    public static ScheduledAdjustmentRequest NewScheduledAdjustmentRequest(string code, string label)
    {
        return new ScheduledAdjustmentRequest(
            code,
            label,
            "",
            "",
            0,
            "",
            0,
            "",
            "",
            "",
            ScheduledAdjustmentRequestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAdjustmentRequestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAdjustmentRequestStatus next)
    {
        switch (Status)
        {
            case ScheduledAdjustmentRequestStatus.Open:
                return next == ScheduledAdjustmentRequestStatus.Resolved;
            case ScheduledAdjustmentRequestStatus.InReview:
                return next == ScheduledAdjustmentRequestStatus.Reopened || next == ScheduledAdjustmentRequestStatus.Archived;
            case ScheduledAdjustmentRequestStatus.Resolved:
                switch (next)
                {
                    case ScheduledAdjustmentRequestStatus.Reopened:
                        return true;
                }
                return false;
            case ScheduledAdjustmentRequestStatus.Reopened:
                switch (next)
                {
                    case ScheduledAdjustmentRequestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledAdjustmentRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled adjustment request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled adjustment request"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Sequence - (double)Limit, 2);
    }

    public ScheduledAdjustmentRequest Copy()
    {
        return new ScheduledAdjustmentRequest(
            Code,
            Label,
            Region,
            Location,
            Limit,
            Badge,
            Sequence,
            OwnerId,
            Channel,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
