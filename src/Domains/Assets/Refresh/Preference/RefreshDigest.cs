namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh digest is in its life cycle.</summary>
public enum RefreshDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One refresh digest, with what the planners need to act on it.</summary>
public sealed class RefreshDigest(string code, string title, double maximum, double weight, string reviewer, string badge, RefreshDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public double Weight { get; set; } = weight;
    public string Reviewer { get; set; } = reviewer;
    public string Badge { get; set; } = badge;
    public RefreshDigestStatus Status { get; set; } = status;

    /// <summary>Starts a refresh digest in status idle.</summary>
    public static RefreshDigest NewRefreshDigest(string code, string title)
    {
        return new RefreshDigest(code, title, 0.0, 0.0, "", "", RefreshDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshDigestStatus next)
    {
        switch (Status)
        {
            case RefreshDigestStatus.Idle:
                switch (next)
                {
                    case RefreshDigestStatus.Stopped:
                        return true;
                }
                return false;
            case RefreshDigestStatus.Warming:
                return next == RefreshDigestStatus.Draining || next == RefreshDigestStatus.Stopped;
            case RefreshDigestStatus.Ready:
                switch (next)
                {
                    case RefreshDigestStatus.Stopped:
                        return true;
                }
                return false;
            case RefreshDigestStatus.Draining:
                return next == RefreshDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RefreshDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Weight + Maximum, 2);
    }

    public RefreshDigest Copy()
    {
        return new RefreshDigest(Code, Title, Maximum, Weight, Reviewer, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
