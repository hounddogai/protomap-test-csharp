namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal feedback window is in its life cycle.</summary>
public enum SeasonalFeedbackWindowStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A seasonal feedback window. Its code is unique within a tenant.</summary>
public sealed class SeasonalFeedbackWindow(string code, string name, string country, int durationMinutes, string currency, string ownerId, int leaveDays, int sequence, string phone, SeasonalFeedbackWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Currency { get; set; } = currency;
    public string OwnerId { get; set; } = ownerId;
    public int LeaveDays { get; set; } = leaveDays;
    public int Sequence { get; set; } = sequence;
    public string Phone { get; set; } = phone;
    public SeasonalFeedbackWindowStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal feedback window in status new.</summary>
    public static SeasonalFeedbackWindow NewSeasonalFeedbackWindow(string code, string name)
    {
        return new SeasonalFeedbackWindow(code, name, "", 0, "", "", 0, 0, "", SeasonalFeedbackWindowStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalFeedbackWindowStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalFeedbackWindowStatus next)
    {
        switch (Status)
        {
            case SeasonalFeedbackWindowStatus.New:
                switch (next)
                {
                    case SeasonalFeedbackWindowStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalFeedbackWindowStatus.Queued:
                return next == SeasonalFeedbackWindowStatus.Done;
            case SeasonalFeedbackWindowStatus.Running:
                return next == SeasonalFeedbackWindowStatus.Failed;
            case SeasonalFeedbackWindowStatus.Done:
                switch (next)
                {
                    case SeasonalFeedbackWindowStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalFeedbackWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal feedback window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)DurationMinutes + (double)LeaveDays, 2);
    }

    public SeasonalFeedbackWindow Copy()
    {
        return new SeasonalFeedbackWindow(
            Code,
            Name,
            Country,
            DurationMinutes,
            Currency,
            OwnerId,
            LeaveDays,
            Sequence,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
