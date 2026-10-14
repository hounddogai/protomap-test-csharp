namespace Acme.Workforce.Compensation.EquityGrant.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant assignment is in its life cycle.</summary>
public enum EquityGrantAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a equity grant assignment and its current status.</summary>
public sealed class EquityGrantAssignment(string code, string name, double discount, double score, double cost, bool active, double hours, int priority, string managerId, EquityGrantAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public double Score { get; set; } = score;
    public double Cost { get; set; } = cost;
    public bool Active { get; set; } = active;
    public double Hours { get; set; } = hours;
    public int Priority { get; set; } = priority;
    public string ManagerId { get; set; } = managerId;
    public EquityGrantAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant assignment in status open.</summary>
    public static EquityGrantAssignment NewEquityGrantAssignment(string code, string name)
    {
        return new EquityGrantAssignment(
            code,
            name,
            0.0,
            0.0,
            0.0,
            false,
            0.0,
            0,
            "",
            EquityGrantAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantAssignmentStatus next)
    {
        switch (Status)
        {
            case EquityGrantAssignmentStatus.Open:
                switch (next)
                {
                    case EquityGrantAssignmentStatus.InReview:
                    case EquityGrantAssignmentStatus.Resolved:
                        return true;
                }
                return false;
            case EquityGrantAssignmentStatus.InReview:
                return next == EquityGrantAssignmentStatus.Reopened;
            case EquityGrantAssignmentStatus.Resolved:
                switch (next)
                {
                    case EquityGrantAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case EquityGrantAssignmentStatus.Reopened:
                switch (next)
                {
                    case EquityGrantAssignmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetLoad()
    {
        return Math.Round(Discount - Hours, 2);
    }

    public EquityGrantAssignment Copy()
    {
        return new EquityGrantAssignment(
            Code,
            Name,
            Discount,
            Score,
            Cost,
            Active,
            Hours,
            Priority,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
