namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile budget is in its life cycle.</summary>
public enum SkillProfileBudgetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A skill profile budget. Its code is unique within a tenant.</summary>
public sealed class SkillProfileBudget(string code, string title, bool approved, double threshold, string jobCode, string managerId, int graceDays, string employeeId, int retries, SkillProfileBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public double Threshold { get; set; } = threshold;
    public string JobCode { get; set; } = jobCode;
    public string ManagerId { get; set; } = managerId;
    public int GraceDays { get; set; } = graceDays;
    public string EmployeeId { get; set; } = employeeId;
    public int Retries { get; set; } = retries;
    public SkillProfileBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile budget in status created.</summary>
    public static SkillProfileBudget NewSkillProfileBudget(string code, string title)
    {
        return new SkillProfileBudget(code, title, false, 0.0, "", "", 0, "", 0, SkillProfileBudgetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileBudgetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileBudgetStatus next)
    {
        switch (Status)
        {
            case SkillProfileBudgetStatus.Created:
                return next == SkillProfileBudgetStatus.Published || next == SkillProfileBudgetStatus.Retired;
            case SkillProfileBudgetStatus.Validated:
                return next == SkillProfileBudgetStatus.Published || next == SkillProfileBudgetStatus.Retired;
            case SkillProfileBudgetStatus.Published:
                switch (next)
                {
                    case SkillProfileBudgetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Threshold * (double)GraceDays, 2);
    }

    public SkillProfileBudget Copy()
    {
        return new SkillProfileBudget(
            Code,
            Title,
            Approved,
            Threshold,
            JobCode,
            ManagerId,
            GraceDays,
            EmployeeId,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
