namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario draft is in its life cycle.</summary>
public enum ScenarioDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a scenario draft and its current status.</summary>
public sealed class ScenarioDraft(string code, string name, int capacity, int batchSize, string team, string channel, string managerId, ScenarioDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int BatchSize { get; set; } = batchSize;
    public string Team { get; set; } = team;
    public string Channel { get; set; } = channel;
    public string ManagerId { get; set; } = managerId;
    public ScenarioDraftStatus Status { get; set; } = status;

    /// <summary>Starts a scenario draft in status open.</summary>
    public static ScenarioDraft NewScenarioDraft(string code, string name)
    {
        return new ScenarioDraft(code, name, 0, 0, "", "", "", ScenarioDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioDraftStatus next)
    {
        switch (Status)
        {
            case ScenarioDraftStatus.Open:
                switch (next)
                {
                    case ScenarioDraftStatus.Resolved:
                        return true;
                }
                return false;
            case ScenarioDraftStatus.InReview:
                return next == ScenarioDraftStatus.Reopened || next == ScenarioDraftStatus.Archived;
            case ScenarioDraftStatus.Resolved:
                switch (next)
                {
                    case ScenarioDraftStatus.Reopened:
                    case ScenarioDraftStatus.Archived:
                        return true;
                }
                return false;
            case ScenarioDraftStatus.Reopened:
                switch (next)
                {
                    case ScenarioDraftStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)BatchSize + (double)Capacity, 2);
    }

    public ScenarioDraft Copy()
    {
        return new ScenarioDraft(Code, Name, Capacity, BatchSize, Team, Channel, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
