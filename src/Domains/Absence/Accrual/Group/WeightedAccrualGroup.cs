namespace Acme.Workforce.Absence.Accrual.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted accrual group is in its life cycle.</summary>
public enum WeightedAccrualGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a weighted accrual group and its current status.</summary>
public sealed class WeightedAccrualGroup(string code, string title, int graceDays, string grade, string description, bool visible, double fte, string costCenter, WeightedAccrualGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public string Grade { get; set; } = grade;
    public string Description { get; set; } = description;
    public bool Visible { get; set; } = visible;
    public double Fte { get; set; } = fte;
    public string CostCenter { get; set; } = costCenter;
    public WeightedAccrualGroupStatus Status { get; set; } = status;

    /// <summary>Starts a weighted accrual group in status planned.</summary>
    public static WeightedAccrualGroup NewWeightedAccrualGroup(string code, string title)
    {
        return new WeightedAccrualGroup(code, title, 0, "", "", false, 0.0, "", WeightedAccrualGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAccrualGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAccrualGroupStatus next)
    {
        switch (Status)
        {
            case WeightedAccrualGroupStatus.Planned:
                return next == WeightedAccrualGroupStatus.Completed || next == WeightedAccrualGroupStatus.Cancelled;
            case WeightedAccrualGroupStatus.Scheduled:
                switch (next)
                {
                    case WeightedAccrualGroupStatus.Cancelled:
                        return true;
                }
                return false;
            case WeightedAccrualGroupStatus.InProgress:
                return next == WeightedAccrualGroupStatus.Completed;
            case WeightedAccrualGroupStatus.Completed:
                return next == WeightedAccrualGroupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WeightedAccrualGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted accrual group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted accrual group"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Fte - (double)GraceDays, 2);
    }

    public WeightedAccrualGroup Copy()
    {
        return new WeightedAccrualGroup(
            Code,
            Title,
            GraceDays,
            Grade,
            Description,
            Visible,
            Fte,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
