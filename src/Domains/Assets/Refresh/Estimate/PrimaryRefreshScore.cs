namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary refresh score is in its life cycle.</summary>
public enum PrimaryRefreshScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One primary refresh score, with what the planners need to act on it.</summary>
public sealed class PrimaryRefreshScore(string code, string title, double weight, int sequence, string contactEmail, string description, PrimaryRefreshScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public int Sequence { get; set; } = sequence;
    public string ContactEmail { get; set; } = contactEmail;
    public string Description { get; set; } = description;
    public PrimaryRefreshScoreStatus Status { get; set; } = status;

    /// <summary>Starts a primary refresh score in status idle.</summary>
    public static PrimaryRefreshScore NewPrimaryRefreshScore(string code, string title)
    {
        return new PrimaryRefreshScore(code, title, 0.0, 0, "", "", PrimaryRefreshScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryRefreshScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryRefreshScoreStatus next)
    {
        switch (Status)
        {
            case PrimaryRefreshScoreStatus.Idle:
                switch (next)
                {
                    case PrimaryRefreshScoreStatus.Ready:
                    case PrimaryRefreshScoreStatus.Draining:
                        return true;
                }
                return false;
            case PrimaryRefreshScoreStatus.Warming:
                switch (next)
                {
                    case PrimaryRefreshScoreStatus.Draining:
                    case PrimaryRefreshScoreStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryRefreshScoreStatus.Ready:
                switch (next)
                {
                    case PrimaryRefreshScoreStatus.Draining:
                    case PrimaryRefreshScoreStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryRefreshScoreStatus.Draining:
                return next == PrimaryRefreshScoreStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PrimaryRefreshScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary refresh score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Sequence + Weight, 2);
    }

    public PrimaryRefreshScore Copy()
    {
        return new PrimaryRefreshScore(Code, Title, Weight, Sequence, ContactEmail, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
