namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant milestone is in its life cycle.</summary>
public enum EquityGrantMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A equity grant milestone as the equity grant team keeps it.</summary>
public sealed class EquityGrantMilestone(string code, string title, int quantity, double hours, int capacity, string grade, EquityGrantMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public double Hours { get; set; } = hours;
    public int Capacity { get; set; } = capacity;
    public string Grade { get; set; } = grade;
    public EquityGrantMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant milestone in status requested.</summary>
    public static EquityGrantMilestone NewEquityGrantMilestone(string code, string title)
    {
        return new EquityGrantMilestone(code, title, 0, 0.0, 0, "", EquityGrantMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantMilestoneStatus next)
    {
        switch (Status)
        {
            case EquityGrantMilestoneStatus.Requested:
                switch (next)
                {
                    case EquityGrantMilestoneStatus.Revoked:
                        return true;
                }
                return false;
            case EquityGrantMilestoneStatus.Granted:
                switch (next)
                {
                    case EquityGrantMilestoneStatus.Revoked:
                        return true;
                }
                return false;
            case EquityGrantMilestoneStatus.Revoked:
                return next == EquityGrantMilestoneStatus.Expired;
        }
        return false;
    }

    public void MoveTo(EquityGrantMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"equity grant milestone"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Capacity - (double)Quantity, 2);
    }

    public EquityGrantMilestone Copy()
    {
        return new EquityGrantMilestone(Code, Title, Quantity, Hours, Capacity, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
