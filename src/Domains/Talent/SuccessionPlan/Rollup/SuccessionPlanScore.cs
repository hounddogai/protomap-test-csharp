namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan score is in its life cycle.</summary>
public enum SuccessionPlanScoreStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One succession plan score, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanScore(string code, string title, List<string> tags, string managerId, int updatedAt, double ratio, SuccessionPlanScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string ManagerId { get; set; } = managerId;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ratio { get; set; } = ratio;
    public SuccessionPlanScoreStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan score in status proposed.</summary>
    public static SuccessionPlanScore NewSuccessionPlanScore(string code, string title)
    {
        return new SuccessionPlanScore(
            code,
            title,
            new List<string>(),
            "",
            0,
            0.0,
            SuccessionPlanScoreStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanScoreStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanScoreStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanScoreStatus.Proposed:
                switch (next)
                {
                    case SuccessionPlanScoreStatus.Approved:
                        return true;
                }
                return false;
            case SuccessionPlanScoreStatus.Approved:
                switch (next)
                {
                    case SuccessionPlanScoreStatus.Rejected:
                    case SuccessionPlanScoreStatus.Withdrawn:
                        return true;
                }
                return false;
            case SuccessionPlanScoreStatus.Rejected:
                return next == SuccessionPlanScoreStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double NetLoad()
    {
        return Math.Round(Ratio * (double)UpdatedAt, 2);
    }

    public SuccessionPlanScore Copy()
    {
        return new SuccessionPlanScore(Code, Title, Tags, ManagerId, UpdatedAt, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
