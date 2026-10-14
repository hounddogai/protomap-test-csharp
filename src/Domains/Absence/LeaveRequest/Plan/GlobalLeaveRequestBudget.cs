namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global leave request budget is in its life cycle.</summary>
public enum GlobalLeaveRequestBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A global leave request budget. Its code is unique within a tenant.</summary>
public sealed class GlobalLeaveRequestBudget(string code, string name, string grade, string externalRef, double score, double ceiling, double ratio, string ownerId, double maximum, List<string> tags, GlobalLeaveRequestBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public string ExternalRef { get; set; } = externalRef;
    public double Score { get; set; } = score;
    public double Ceiling { get; set; } = ceiling;
    public double Ratio { get; set; } = ratio;
    public string OwnerId { get; set; } = ownerId;
    public double Maximum { get; set; } = maximum;
    public List<string> Tags { get; set; } = tags;
    public GlobalLeaveRequestBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a global leave request budget in status draft.</summary>
    public static GlobalLeaveRequestBudget NewGlobalLeaveRequestBudget(string code, string name)
    {
        return new GlobalLeaveRequestBudget(
            code,
            name,
            "",
            "",
            0.0,
            0.0,
            0.0,
            "",
            0.0,
            new List<string>(),
            GlobalLeaveRequestBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalLeaveRequestBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalLeaveRequestBudgetStatus next)
    {
        switch (Status)
        {
            case GlobalLeaveRequestBudgetStatus.Draft:
                return next == GlobalLeaveRequestBudgetStatus.Active || next == GlobalLeaveRequestBudgetStatus.Suspended;
            case GlobalLeaveRequestBudgetStatus.Pending:
                switch (next)
                {
                    case GlobalLeaveRequestBudgetStatus.Active:
                        return true;
                }
                return false;
            case GlobalLeaveRequestBudgetStatus.Active:
                switch (next)
                {
                    case GlobalLeaveRequestBudgetStatus.Suspended:
                        return true;
                }
                return false;
            case GlobalLeaveRequestBudgetStatus.Suspended:
                return next == GlobalLeaveRequestBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GlobalLeaveRequestBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global leave request budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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
        return Math.Round(Ratio + Ceiling, 2);
    }

    public GlobalLeaveRequestBudget Copy()
    {
        return new GlobalLeaveRequestBudget(
            Code,
            Name,
            Grade,
            ExternalRef,
            Score,
            Ceiling,
            Ratio,
            OwnerId,
            Maximum,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
