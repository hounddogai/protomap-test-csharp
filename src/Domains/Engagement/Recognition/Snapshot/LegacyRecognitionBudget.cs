namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy recognition budget is in its life cycle.</summary>
public enum LegacyRecognitionBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One legacy recognition budget, with what the planners need to act on it.</summary>
public sealed class LegacyRecognitionBudget(string code, string name, List<string> tags, double budget, int retries, string team, int updatedAt, string reviewer, LegacyRecognitionBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public double Budget { get; set; } = budget;
    public int Retries { get; set; } = retries;
    public string Team { get; set; } = team;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Reviewer { get; set; } = reviewer;
    public LegacyRecognitionBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a legacy recognition budget in status planned.</summary>
    public static LegacyRecognitionBudget NewLegacyRecognitionBudget(string code, string name)
    {
        return new LegacyRecognitionBudget(
            code,
            name,
            new List<string>(),
            0.0,
            0,
            "",
            0,
            "",
            LegacyRecognitionBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRecognitionBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRecognitionBudgetStatus next)
    {
        switch (Status)
        {
            case LegacyRecognitionBudgetStatus.Planned:
                return next == LegacyRecognitionBudgetStatus.Completed;
            case LegacyRecognitionBudgetStatus.Scheduled:
                return next == LegacyRecognitionBudgetStatus.Completed || next == LegacyRecognitionBudgetStatus.Cancelled;
            case LegacyRecognitionBudgetStatus.InProgress:
                return next == LegacyRecognitionBudgetStatus.Completed || next == LegacyRecognitionBudgetStatus.Cancelled;
            case LegacyRecognitionBudgetStatus.Completed:
                return next == LegacyRecognitionBudgetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LegacyRecognitionBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy recognition budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"legacy recognition budget"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedLoad()
    {
        return Math.Round((double)UpdatedAt + Budget, 2);
    }

    public LegacyRecognitionBudget Copy()
    {
        return new LegacyRecognitionBudget(Code, Name, Tags, Budget, Retries, Team, UpdatedAt, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
