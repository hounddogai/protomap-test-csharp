namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global career path rule is in its life cycle.</summary>
public enum GlobalCareerPathRuleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a global career path rule and its current status.</summary>
public sealed class GlobalCareerPathRule(string code, string label, string segment, int leaveDays, List<string> tags, int updatedAt, string channel, double cost, string name, double budget, GlobalCareerPathRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public int LeaveDays { get; set; } = leaveDays;
    public List<string> Tags { get; set; } = tags;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Channel { get; set; } = channel;
    public double Cost { get; set; } = cost;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public GlobalCareerPathRuleStatus Status { get; set; } = status;

    /// <summary>Starts a global career path rule in status idle.</summary>
    public static GlobalCareerPathRule NewGlobalCareerPathRule(string code, string label)
    {
        return new GlobalCareerPathRule(
            code,
            label,
            "",
            0,
            new List<string>(),
            0,
            "",
            0.0,
            "",
            0.0,
            GlobalCareerPathRuleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalCareerPathRuleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalCareerPathRuleStatus next)
    {
        switch (Status)
        {
            case GlobalCareerPathRuleStatus.Idle:
                switch (next)
                {
                    case GlobalCareerPathRuleStatus.Warming:
                        return true;
                }
                return false;
            case GlobalCareerPathRuleStatus.Warming:
                return next == GlobalCareerPathRuleStatus.Stopped;
            case GlobalCareerPathRuleStatus.Ready:
                return next == GlobalCareerPathRuleStatus.Draining || next == GlobalCareerPathRuleStatus.Stopped;
            case GlobalCareerPathRuleStatus.Draining:
                return next == GlobalCareerPathRuleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalCareerPathRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global career path rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"global career path rule"} {Code}";
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
        return Math.Round(Cost - (double)LeaveDays, 2);
    }

    public GlobalCareerPathRule Copy()
    {
        return new GlobalCareerPathRule(
            Code,
            Label,
            Segment,
            LeaveDays,
            Tags,
            UpdatedAt,
            Channel,
            Cost,
            Name,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
