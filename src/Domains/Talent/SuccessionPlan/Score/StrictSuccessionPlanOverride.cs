namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict succession plan override is in its life cycle.</summary>
public enum StrictSuccessionPlanOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One strict succession plan override, with what the planners need to act on it.</summary>
public sealed class StrictSuccessionPlanOverride(string code, string title, string category, string skill, string salaryBand, string contactEmail, int version, double score, bool archived, double floorValue, StrictSuccessionPlanOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Skill { get; set; } = skill;
    public string SalaryBand { get; set; } = salaryBand;
    public string ContactEmail { get; set; } = contactEmail;
    public int Version { get; set; } = version;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public double FloorValue { get; set; } = floorValue;
    public StrictSuccessionPlanOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a strict succession plan override in status requested.</summary>
    public static StrictSuccessionPlanOverride NewStrictSuccessionPlanOverride(string code, string title)
    {
        return new StrictSuccessionPlanOverride(
            code,
            title,
            "",
            "",
            "",
            "",
            0,
            0.0,
            false,
            0.0,
            StrictSuccessionPlanOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictSuccessionPlanOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictSuccessionPlanOverrideStatus next)
    {
        switch (Status)
        {
            case StrictSuccessionPlanOverrideStatus.Requested:
                switch (next)
                {
                    case StrictSuccessionPlanOverrideStatus.Granted:
                    case StrictSuccessionPlanOverrideStatus.Expired:
                        return true;
                }
                return false;
            case StrictSuccessionPlanOverrideStatus.Granted:
                switch (next)
                {
                    case StrictSuccessionPlanOverrideStatus.Expired:
                        return true;
                }
                return false;
            case StrictSuccessionPlanOverrideStatus.Revoked:
                switch (next)
                {
                    case StrictSuccessionPlanOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictSuccessionPlanOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict succession plan override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Version - FloorValue, 2);
    }

    public StrictSuccessionPlanOverride Copy()
    {
        return new StrictSuccessionPlanOverride(
            Code,
            Title,
            Category,
            Skill,
            SalaryBand,
            ContactEmail,
            Version,
            Score,
            Archived,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
