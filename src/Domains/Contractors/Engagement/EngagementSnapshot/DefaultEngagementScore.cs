namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default engagement score is in its life cycle.</summary>
public enum DefaultEngagementScoreStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a default engagement score and its current status.</summary>
public sealed class DefaultEngagementScore(string code, string name, string segment, double rating, bool visible, DefaultEngagementScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public double Rating { get; set; } = rating;
    public bool Visible { get; set; } = visible;
    public DefaultEngagementScoreStatus Status { get; set; } = status;

    /// <summary>Starts a default engagement score in status planned.</summary>
    public static DefaultEngagementScore NewDefaultEngagementScore(string code, string name)
    {
        return new DefaultEngagementScore(code, name, "", 0.0, false, DefaultEngagementScoreStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultEngagementScoreStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultEngagementScoreStatus next)
    {
        switch (Status)
        {
            case DefaultEngagementScoreStatus.Planned:
                return next == DefaultEngagementScoreStatus.Scheduled || next == DefaultEngagementScoreStatus.Completed;
            case DefaultEngagementScoreStatus.Scheduled:
                switch (next)
                {
                    case DefaultEngagementScoreStatus.InProgress:
                    case DefaultEngagementScoreStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultEngagementScoreStatus.InProgress:
                return next == DefaultEngagementScoreStatus.Cancelled;
            case DefaultEngagementScoreStatus.Completed:
                switch (next)
                {
                    case DefaultEngagementScoreStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultEngagementScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default engagement score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public DefaultEngagementScore Copy()
    {
        return new DefaultEngagementScore(Code, Name, Segment, Rating, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
