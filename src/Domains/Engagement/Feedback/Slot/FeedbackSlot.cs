namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback slot is in its life cycle.</summary>
public enum FeedbackSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a feedback slot and its current status.</summary>
public sealed class FeedbackSlot(string code, string name, string channel, int limit, double maximum, string title, string costCenter, FeedbackSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public int Limit { get; set; } = limit;
    public double Maximum { get; set; } = maximum;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public FeedbackSlotStatus Status { get; set; } = status;

    /// <summary>Starts a feedback slot in status planned.</summary>
    public static FeedbackSlot NewFeedbackSlot(string code, string name)
    {
        return new FeedbackSlot(code, name, "", 0, 0.0, "", "", FeedbackSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackSlotStatus next)
    {
        switch (Status)
        {
            case FeedbackSlotStatus.Planned:
                switch (next)
                {
                    case FeedbackSlotStatus.Scheduled:
                    case FeedbackSlotStatus.Cancelled:
                        return true;
                }
                return false;
            case FeedbackSlotStatus.Scheduled:
                switch (next)
                {
                    case FeedbackSlotStatus.InProgress:
                    case FeedbackSlotStatus.Completed:
                        return true;
                }
                return false;
            case FeedbackSlotStatus.InProgress:
                switch (next)
                {
                    case FeedbackSlotStatus.Completed:
                    case FeedbackSlotStatus.Cancelled:
                        return true;
                }
                return false;
            case FeedbackSlotStatus.Completed:
                switch (next)
                {
                    case FeedbackSlotStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Limit + Maximum, 2);
    }

    public FeedbackSlot Copy()
    {
        return new FeedbackSlot(Code, Name, Channel, Limit, Maximum, Title, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
