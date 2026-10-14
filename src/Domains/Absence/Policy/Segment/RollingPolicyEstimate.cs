namespace Acme.Workforce.Absence.Policy.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling policy estimate is in its life cycle.</summary>
public enum RollingPolicyEstimateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One rolling policy estimate, with what the planners need to act on it.</summary>
public sealed class RollingPolicyEstimate(string code, string title, int priority, int attempts, string skill, string currency, RollingPolicyEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public int Attempts { get; set; } = attempts;
    public string Skill { get; set; } = skill;
    public string Currency { get; set; } = currency;
    public RollingPolicyEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a rolling policy estimate in status new.</summary>
    public static RollingPolicyEstimate NewRollingPolicyEstimate(string code, string title)
    {
        return new RollingPolicyEstimate(code, title, 0, 0, "", "", RollingPolicyEstimateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPolicyEstimateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPolicyEstimateStatus next)
    {
        switch (Status)
        {
            case RollingPolicyEstimateStatus.New:
                switch (next)
                {
                    case RollingPolicyEstimateStatus.Queued:
                        return true;
                }
                return false;
            case RollingPolicyEstimateStatus.Queued:
                switch (next)
                {
                    case RollingPolicyEstimateStatus.Running:
                    case RollingPolicyEstimateStatus.Done:
                        return true;
                }
                return false;
            case RollingPolicyEstimateStatus.Running:
                switch (next)
                {
                    case RollingPolicyEstimateStatus.Done:
                    case RollingPolicyEstimateStatus.Failed:
                        return true;
                }
                return false;
            case RollingPolicyEstimateStatus.Done:
                switch (next)
                {
                    case RollingPolicyEstimateStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPolicyEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling policy estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling policy estimate"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Attempts - (double)Priority, 2);
    }

    public RollingPolicyEstimate Copy()
    {
        return new RollingPolicyEstimate(Code, Title, Priority, Attempts, Skill, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
