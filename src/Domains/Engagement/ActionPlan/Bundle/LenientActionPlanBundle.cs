namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient action plan bundle is in its life cycle.</summary>
public enum LenientActionPlanBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a lenient action plan bundle and its current status.</summary>
public sealed class LenientActionPlanBundle(string code, string name, int graceDays, string employeeId, int capacity, string location, int tenureMonths, bool visible, string skill, int batchSize, LenientActionPlanBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public string EmployeeId { get; set; } = employeeId;
    public int Capacity { get; set; } = capacity;
    public string Location { get; set; } = location;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Visible { get; set; } = visible;
    public string Skill { get; set; } = skill;
    public int BatchSize { get; set; } = batchSize;
    public LenientActionPlanBundleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient action plan bundle in status draft.</summary>
    public static LenientActionPlanBundle NewLenientActionPlanBundle(string code, string name)
    {
        return new LenientActionPlanBundle(
            code,
            name,
            0,
            "",
            0,
            "",
            0,
            false,
            "",
            0,
            LenientActionPlanBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientActionPlanBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientActionPlanBundleStatus next)
    {
        switch (Status)
        {
            case LenientActionPlanBundleStatus.Draft:
                switch (next)
                {
                    case LenientActionPlanBundleStatus.Pending:
                    case LenientActionPlanBundleStatus.Suspended:
                        return true;
                }
                return false;
            case LenientActionPlanBundleStatus.Pending:
                switch (next)
                {
                    case LenientActionPlanBundleStatus.Active:
                    case LenientActionPlanBundleStatus.Closed:
                        return true;
                }
                return false;
            case LenientActionPlanBundleStatus.Active:
                switch (next)
                {
                    case LenientActionPlanBundleStatus.Suspended:
                    case LenientActionPlanBundleStatus.Closed:
                        return true;
                }
                return false;
            case LenientActionPlanBundleStatus.Suspended:
                return next == LenientActionPlanBundleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LenientActionPlanBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient action plan bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient action plan bundle"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)TenureMonths + (double)GraceDays, 2);
    }

    public LenientActionPlanBundle Copy()
    {
        return new LenientActionPlanBundle(
            Code,
            Name,
            GraceDays,
            EmployeeId,
            Capacity,
            Location,
            TenureMonths,
            Visible,
            Skill,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
