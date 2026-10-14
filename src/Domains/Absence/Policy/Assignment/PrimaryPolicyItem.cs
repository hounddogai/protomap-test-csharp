namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary policy item is in its life cycle.</summary>
public enum PrimaryPolicyItemStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One primary policy item, with what the planners need to act on it.</summary>
public sealed class PrimaryPolicyItem(string code, string name, double weight, string badge, string department, string segment, double score, double floorValue, PrimaryPolicyItemStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Badge { get; set; } = badge;
    public string Department { get; set; } = department;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public double FloorValue { get; set; } = floorValue;
    public PrimaryPolicyItemStatus Status { get; set; } = status;

    /// <summary>Starts a primary policy item in status planned.</summary>
    public static PrimaryPolicyItem NewPrimaryPolicyItem(string code, string name)
    {
        return new PrimaryPolicyItem(code, name, 0.0, "", "", "", 0.0, 0.0, PrimaryPolicyItemStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPolicyItemStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPolicyItemStatus next)
    {
        switch (Status)
        {
            case PrimaryPolicyItemStatus.Planned:
                switch (next)
                {
                    case PrimaryPolicyItemStatus.InProgress:
                    case PrimaryPolicyItemStatus.Completed:
                        return true;
                }
                return false;
            case PrimaryPolicyItemStatus.Scheduled:
                return next == PrimaryPolicyItemStatus.Completed || next == PrimaryPolicyItemStatus.Cancelled;
            case PrimaryPolicyItemStatus.InProgress:
                switch (next)
                {
                    case PrimaryPolicyItemStatus.Completed:
                    case PrimaryPolicyItemStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryPolicyItemStatus.Completed:
                switch (next)
                {
                    case PrimaryPolicyItemStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryPolicyItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary policy item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetValue()
    {
        return Math.Round(Weight * Score, 2);
    }

    public PrimaryPolicyItem Copy()
    {
        return new PrimaryPolicyItem(Code, Name, Weight, Badge, Department, Segment, Score, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
