namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement budget is in its life cycle.</summary>
public enum EngagementBudgetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One engagement budget, with what the planners need to act on it.</summary>
public sealed class EngagementBudget(string code, string title, string managerId, double rate, string name, bool approved, bool archived, int sequence, int version, int batchSize, EngagementBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public double Rate { get; set; } = rate;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public bool Archived { get; set; } = archived;
    public int Sequence { get; set; } = sequence;
    public int Version { get; set; } = version;
    public int BatchSize { get; set; } = batchSize;
    public EngagementBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a engagement budget in status open.</summary>
    public static EngagementBudget NewEngagementBudget(string code, string title)
    {
        return new EngagementBudget(code, title, "", 0.0, "", false, false, 0, 0, 0, EngagementBudgetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementBudgetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementBudgetStatus next)
    {
        switch (Status)
        {
            case EngagementBudgetStatus.Open:
                switch (next)
                {
                    case EngagementBudgetStatus.Resolved:
                    case EngagementBudgetStatus.Reopened:
                        return true;
                }
                return false;
            case EngagementBudgetStatus.InReview:
                return next == EngagementBudgetStatus.Reopened || next == EngagementBudgetStatus.Archived;
            case EngagementBudgetStatus.Resolved:
                switch (next)
                {
                    case EngagementBudgetStatus.Archived:
                        return true;
                }
                return false;
            case EngagementBudgetStatus.Reopened:
                switch (next)
                {
                    case EngagementBudgetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Version - Rate, 2);
    }

    public EngagementBudget Copy()
    {
        return new EngagementBudget(
            Code,
            Title,
            ManagerId,
            Rate,
            Name,
            Approved,
            Archived,
            Sequence,
            Version,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
