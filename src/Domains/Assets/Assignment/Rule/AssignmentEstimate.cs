namespace Acme.Workforce.Assets.Assignment.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment estimate is in its life cycle.</summary>
public enum AssignmentEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a assignment estimate and its current status.</summary>
public sealed class AssignmentEstimate(string code, string name, int version, double discount, string costCenter, double cost, int quantity, int windowDays, AssignmentEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public double Discount { get; set; } = discount;
    public string CostCenter { get; set; } = costCenter;
    public double Cost { get; set; } = cost;
    public int Quantity { get; set; } = quantity;
    public int WindowDays { get; set; } = windowDays;
    public AssignmentEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a assignment estimate in status planned.</summary>
    public static AssignmentEstimate NewAssignmentEstimate(string code, string name)
    {
        return new AssignmentEstimate(code, name, 0, 0.0, "", 0.0, 0, 0, AssignmentEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentEstimateStatus next)
    {
        switch (Status)
        {
            case AssignmentEstimateStatus.Planned:
                return next == AssignmentEstimateStatus.Scheduled;
            case AssignmentEstimateStatus.Scheduled:
                switch (next)
                {
                    case AssignmentEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case AssignmentEstimateStatus.InProgress:
                return next == AssignmentEstimateStatus.Completed;
            case AssignmentEstimateStatus.Completed:
                switch (next)
                {
                    case AssignmentEstimateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Quantity - (double)Version, 2);
    }

    public AssignmentEstimate Copy()
    {
        return new AssignmentEstimate(Code, Name, Version, Discount, CostCenter, Cost, Quantity, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
