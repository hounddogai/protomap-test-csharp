namespace Acme.Workforce.Orgdesign.Scenario.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario notice is in its life cycle.</summary>
public enum ScenarioNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scenario notice. Its code is unique within a tenant.</summary>
public sealed class ScenarioNotice(string code, string title, string managerId, double weight, int attempts, int priority, List<string> tags, ScenarioNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public double Weight { get; set; } = weight;
    public int Attempts { get; set; } = attempts;
    public int Priority { get; set; } = priority;
    public List<string> Tags { get; set; } = tags;
    public ScenarioNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a scenario notice in status open.</summary>
    public static ScenarioNotice NewScenarioNotice(string code, string title)
    {
        return new ScenarioNotice(code, title, "", 0.0, 0, 0, new List<string>(), ScenarioNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioNoticeStatus next)
    {
        switch (Status)
        {
            case ScenarioNoticeStatus.Open:
                return next == ScenarioNoticeStatus.Resolved || next == ScenarioNoticeStatus.Reopened;
            case ScenarioNoticeStatus.InReview:
                return next == ScenarioNoticeStatus.Resolved || next == ScenarioNoticeStatus.Archived;
            case ScenarioNoticeStatus.Resolved:
                return next == ScenarioNoticeStatus.Reopened || next == ScenarioNoticeStatus.Archived;
            case ScenarioNoticeStatus.Reopened:
                switch (next)
                {
                    case ScenarioNoticeStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double ProjectedExposure()
    {
        return Math.Round(Weight + (double)Attempts, 2);
    }

    public ScenarioNotice Copy()
    {
        return new ScenarioNotice(Code, Title, ManagerId, Weight, Attempts, Priority, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
