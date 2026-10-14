namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual leave request score is in its life cycle.</summary>
public enum ManualLeaveRequestScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a manual leave request score and its current status.</summary>
public sealed class ManualLeaveRequestScore(string code, string name, List<string> tags, string jobCode, string category, string skill, int leaveDays, int windowDays, string unit, double ratio, ManualLeaveRequestScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string JobCode { get; set; } = jobCode;
    public string Category { get; set; } = category;
    public string Skill { get; set; } = skill;
    public int LeaveDays { get; set; } = leaveDays;
    public int WindowDays { get; set; } = windowDays;
    public string Unit { get; set; } = unit;
    public double Ratio { get; set; } = ratio;
    public ManualLeaveRequestScoreStatus Status { get; set; } = status;

    /// <summary>Starts a manual leave request score in status requested.</summary>
    public static ManualLeaveRequestScore NewManualLeaveRequestScore(string code, string name)
    {
        return new ManualLeaveRequestScore(
            code,
            name,
            new List<string>(),
            "",
            "",
            "",
            0,
            0,
            "",
            0.0,
            ManualLeaveRequestScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualLeaveRequestScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualLeaveRequestScoreStatus next)
    {
        switch (Status)
        {
            case ManualLeaveRequestScoreStatus.Requested:
                switch (next)
                {
                    case ManualLeaveRequestScoreStatus.Revoked:
                    case ManualLeaveRequestScoreStatus.Expired:
                        return true;
                }
                return false;
            case ManualLeaveRequestScoreStatus.Granted:
                switch (next)
                {
                    case ManualLeaveRequestScoreStatus.Revoked:
                    case ManualLeaveRequestScoreStatus.Expired:
                        return true;
                }
                return false;
            case ManualLeaveRequestScoreStatus.Revoked:
                switch (next)
                {
                    case ManualLeaveRequestScoreStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualLeaveRequestScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual leave request score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual leave request score"} {Code}";
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

    public double WeightedValue()
    {
        return Math.Round((double)LeaveDays - Ratio, 2);
    }

    public ManualLeaveRequestScore Copy()
    {
        return new ManualLeaveRequestScore(
            Code,
            Name,
            Tags,
            JobCode,
            Category,
            Skill,
            LeaveDays,
            WindowDays,
            Unit,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
