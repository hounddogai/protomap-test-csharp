namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount adjustment is in its life cycle.</summary>
public enum HeadcountAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a headcount adjustment and its current status.</summary>
public sealed class HeadcountAdjustment(string code, string title, string name, string employeeId, double minimum, double margin, HeadcountAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public double Minimum { get; set; } = minimum;
    public double Margin { get; set; } = margin;
    public HeadcountAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a headcount adjustment in status planned.</summary>
    public static HeadcountAdjustment NewHeadcountAdjustment(string code, string title)
    {
        return new HeadcountAdjustment(code, title, "", "", 0.0, 0.0, HeadcountAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountAdjustmentStatus next)
    {
        switch (Status)
        {
            case HeadcountAdjustmentStatus.Planned:
                switch (next)
                {
                    case HeadcountAdjustmentStatus.InProgress:
                    case HeadcountAdjustmentStatus.Completed:
                        return true;
                }
                return false;
            case HeadcountAdjustmentStatus.Scheduled:
                return next == HeadcountAdjustmentStatus.InProgress || next == HeadcountAdjustmentStatus.Cancelled;
            case HeadcountAdjustmentStatus.InProgress:
                switch (next)
                {
                    case HeadcountAdjustmentStatus.Completed:
                    case HeadcountAdjustmentStatus.Cancelled:
                        return true;
                }
                return false;
            case HeadcountAdjustmentStatus.Completed:
                return next == HeadcountAdjustmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(HeadcountAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"headcount adjustment"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Margin - Minimum, 2);
    }

    public HeadcountAdjustment Copy()
    {
        return new HeadcountAdjustment(Code, Title, Name, EmployeeId, Minimum, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
