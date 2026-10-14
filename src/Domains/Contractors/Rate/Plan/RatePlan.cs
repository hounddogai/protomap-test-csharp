namespace Acme.Workforce.Contractors.Rate.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate plan is in its life cycle.</summary>
public enum RatePlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a rate plan and its current status.</summary>
public sealed class RatePlan(string code, string label, string skill, List<string> tags, double maximum, string unit, string salaryBand, string location, RatePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public List<string> Tags { get; set; } = tags;
    public double Maximum { get; set; } = maximum;
    public string Unit { get; set; } = unit;
    public string SalaryBand { get; set; } = salaryBand;
    public string Location { get; set; } = location;
    public RatePlanStatus Status { get; set; } = status;

    /// <summary>Starts a rate plan in status new.</summary>
    public static RatePlan NewRatePlan(string code, string label)
    {
        return new RatePlan(code, label, "", new List<string>(), 0.0, "", "", "", RatePlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RatePlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RatePlanStatus next)
    {
        switch (Status)
        {
            case RatePlanStatus.New:
                return next == RatePlanStatus.Done;
            case RatePlanStatus.Queued:
                return next == RatePlanStatus.Running;
            case RatePlanStatus.Running:
                return next == RatePlanStatus.Done;
            case RatePlanStatus.Done:
                return next == RatePlanStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RatePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rate plan"} {Code}";
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

    public double MeasuredValue()
    {
        return Maximum;
    }

    public RatePlan Copy()
    {
        return new RatePlan(Code, Label, Skill, Tags, Maximum, Unit, SalaryBand, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
