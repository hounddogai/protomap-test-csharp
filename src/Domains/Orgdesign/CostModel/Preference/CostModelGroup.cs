namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model group is in its life cycle.</summary>
public enum CostModelGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A cost model group as the cost model team keeps it.</summary>
public sealed class CostModelGroup(string code, string name, double ceiling, int priority, int tenureMonths, double maximum, CostModelGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public int Priority { get; set; } = priority;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Maximum { get; set; } = maximum;
    public CostModelGroupStatus Status { get; set; } = status;

    /// <summary>Starts a cost model group in status open.</summary>
    public static CostModelGroup NewCostModelGroup(string code, string name)
    {
        return new CostModelGroup(code, name, 0.0, 0, 0, 0.0, CostModelGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelGroupStatus next)
    {
        switch (Status)
        {
            case CostModelGroupStatus.Open:
                return next == CostModelGroupStatus.InReview;
            case CostModelGroupStatus.InReview:
                return next == CostModelGroupStatus.Resolved;
            case CostModelGroupStatus.Resolved:
                return next == CostModelGroupStatus.Archived;
            case CostModelGroupStatus.Reopened:
                return next == CostModelGroupStatus.Archived;
        }
        return false;
    }

    public void MoveTo(CostModelGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetValue()
    {
        return Math.Round((double)TenureMonths - (double)Priority, 2);
    }

    public CostModelGroup Copy()
    {
        return new CostModelGroup(Code, Name, Ceiling, Priority, TenureMonths, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
