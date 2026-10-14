namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global engagement score is in its life cycle.</summary>
public enum GlobalEngagementScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a global engagement score and its current status.</summary>
public sealed class GlobalEngagementScore(string code, string title, double maximum, int endsAt, int createdAt, string contactEmail, GlobalEngagementScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public int EndsAt { get; set; } = endsAt;
    public int CreatedAt { get; set; } = createdAt;
    public string ContactEmail { get; set; } = contactEmail;
    public GlobalEngagementScoreStatus Status { get; set; } = status;

    /// <summary>Starts a global engagement score in status idle.</summary>
    public static GlobalEngagementScore NewGlobalEngagementScore(string code, string title)
    {
        return new GlobalEngagementScore(code, title, 0.0, 0, 0, "", GlobalEngagementScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalEngagementScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalEngagementScoreStatus next)
    {
        switch (Status)
        {
            case GlobalEngagementScoreStatus.Idle:
                return next == GlobalEngagementScoreStatus.Draining || next == GlobalEngagementScoreStatus.Stopped;
            case GlobalEngagementScoreStatus.Warming:
                switch (next)
                {
                    case GlobalEngagementScoreStatus.Ready:
                        return true;
                }
                return false;
            case GlobalEngagementScoreStatus.Ready:
                switch (next)
                {
                    case GlobalEngagementScoreStatus.Draining:
                    case GlobalEngagementScoreStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalEngagementScoreStatus.Draining:
                switch (next)
                {
                    case GlobalEngagementScoreStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalEngagementScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global engagement score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)EndsAt + Maximum, 2);
    }

    public GlobalEngagementScore Copy()
    {
        return new GlobalEngagementScore(Code, Title, Maximum, EndsAt, CreatedAt, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
