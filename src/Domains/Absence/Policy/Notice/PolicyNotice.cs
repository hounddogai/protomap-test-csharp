namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy notice is in its life cycle.</summary>
public enum PolicyNoticeStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a policy notice and its current status.</summary>
public sealed class PolicyNotice(string code, string name, string location, double weight, string jobCode, string skill, int windowDays, string segment, PolicyNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public double Weight { get; set; } = weight;
    public string JobCode { get; set; } = jobCode;
    public string Skill { get; set; } = skill;
    public int WindowDays { get; set; } = windowDays;
    public string Segment { get; set; } = segment;
    public PolicyNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a policy notice in status open.</summary>
    public static PolicyNotice NewPolicyNotice(string code, string name)
    {
        return new PolicyNotice(code, name, "", 0.0, "", "", 0, "", PolicyNoticeStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyNoticeStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyNoticeStatus next)
    {
        switch (Status)
        {
            case PolicyNoticeStatus.Open:
                return next == PolicyNoticeStatus.Resolved || next == PolicyNoticeStatus.Reopened;
            case PolicyNoticeStatus.InReview:
                switch (next)
                {
                    case PolicyNoticeStatus.Archived:
                        return true;
                }
                return false;
            case PolicyNoticeStatus.Resolved:
                switch (next)
                {
                    case PolicyNoticeStatus.Reopened:
                    case PolicyNoticeStatus.Archived:
                        return true;
                }
                return false;
            case PolicyNoticeStatus.Reopened:
                switch (next)
                {
                    case PolicyNoticeStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy notice"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)WindowDays - Weight, 2);
    }

    public PolicyNotice Copy()
    {
        return new PolicyNotice(Code, Name, Location, Weight, JobCode, Skill, WindowDays, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
