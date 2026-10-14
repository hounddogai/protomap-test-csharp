namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work rollup is in its life cycle.</summary>
public enum StatementOfWorkRollupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One statement of work rollup, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkRollup(string code, string label, string region, int limit, string skill, int expiresAt, StatementOfWorkRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public int Limit { get; set; } = limit;
    public string Skill { get; set; } = skill;
    public int ExpiresAt { get; set; } = expiresAt;
    public StatementOfWorkRollupStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work rollup in status idle.</summary>
    public static StatementOfWorkRollup NewStatementOfWorkRollup(string code, string label)
    {
        return new StatementOfWorkRollup(code, label, "", 0, "", 0, StatementOfWorkRollupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkRollupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkRollupStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkRollupStatus.Idle:
                return next == StatementOfWorkRollupStatus.Draining;
            case StatementOfWorkRollupStatus.Warming:
                return next == StatementOfWorkRollupStatus.Ready || next == StatementOfWorkRollupStatus.Draining;
            case StatementOfWorkRollupStatus.Ready:
                switch (next)
                {
                    case StatementOfWorkRollupStatus.Draining:
                    case StatementOfWorkRollupStatus.Stopped:
                        return true;
                }
                return false;
            case StatementOfWorkRollupStatus.Draining:
                return next == StatementOfWorkRollupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"statement of work rollup"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)ExpiresAt + (double)Limit, 2);
    }

    public StatementOfWorkRollup Copy()
    {
        return new StatementOfWorkRollup(Code, Label, Region, Limit, Skill, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
