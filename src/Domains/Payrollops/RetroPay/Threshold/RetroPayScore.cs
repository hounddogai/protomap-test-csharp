namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay score is in its life cycle.</summary>
public enum RetroPayScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A retro pay score as the retro pay team keeps it.</summary>
public sealed class RetroPayScore(string code, string label, int capacity, string badge, string jobCode, string team, int limit, RetroPayScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public string Badge { get; set; } = badge;
    public string JobCode { get; set; } = jobCode;
    public string Team { get; set; } = team;
    public int Limit { get; set; } = limit;
    public RetroPayScoreStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay score in status open.</summary>
    public static RetroPayScore NewRetroPayScore(string code, string label)
    {
        return new RetroPayScore(code, label, 0, "", "", "", 0, RetroPayScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayScoreStatus next)
    {
        switch (Status)
        {
            case RetroPayScoreStatus.Open:
                return next == RetroPayScoreStatus.Reopened;
            case RetroPayScoreStatus.InReview:
                switch (next)
                {
                    case RetroPayScoreStatus.Reopened:
                    case RetroPayScoreStatus.Archived:
                        return true;
                }
                return false;
            case RetroPayScoreStatus.Resolved:
                switch (next)
                {
                    case RetroPayScoreStatus.Archived:
                        return true;
                }
                return false;
            case RetroPayScoreStatus.Reopened:
                switch (next)
                {
                    case RetroPayScoreStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Capacity * (double)Limit, 2);
    }

    public RetroPayScore Copy()
    {
        return new RetroPayScore(Code, Label, Capacity, Badge, JobCode, Team, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
