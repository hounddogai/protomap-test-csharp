namespace Acme.Workforce.Talent.SuccessionPlan.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan rule is in its life cycle.</summary>
public enum SuccessionPlanRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A succession plan rule as the succession plan team keeps it.</summary>
public sealed class SuccessionPlanRule(string code, string name, string department, string jobCode, int limit, SuccessionPlanRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string JobCode { get; set; } = jobCode;
    public int Limit { get; set; } = limit;
    public SuccessionPlanRuleStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan rule in status new.</summary>
    public static SuccessionPlanRule NewSuccessionPlanRule(string code, string name)
    {
        return new SuccessionPlanRule(code, name, "", "", 0, SuccessionPlanRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanRuleStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanRuleStatus.New:
                return next == SuccessionPlanRuleStatus.Queued || next == SuccessionPlanRuleStatus.Done;
            case SuccessionPlanRuleStatus.Queued:
                switch (next)
                {
                    case SuccessionPlanRuleStatus.Failed:
                        return true;
                }
                return false;
            case SuccessionPlanRuleStatus.Running:
                switch (next)
                {
                    case SuccessionPlanRuleStatus.Done:
                        return true;
                }
                return false;
            case SuccessionPlanRuleStatus.Done:
                return next == SuccessionPlanRuleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public SuccessionPlanRule Copy()
    {
        return new SuccessionPlanRule(Code, Name, Department, JobCode, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
