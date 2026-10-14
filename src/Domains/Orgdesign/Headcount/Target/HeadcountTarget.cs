namespace Acme.Workforce.Orgdesign.Headcount.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount target is in its life cycle.</summary>
public enum HeadcountTargetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A headcount target as the headcount team keeps it.</summary>
public sealed class HeadcountTarget(string code, string label, double cost, int retries, string department, HeadcountTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public int Retries { get; set; } = retries;
    public string Department { get; set; } = department;
    public HeadcountTargetStatus Status { get; set; } = status;

    /// <summary>Starts a headcount target in status proposed.</summary>
    public static HeadcountTarget NewHeadcountTarget(string code, string label)
    {
        return new HeadcountTarget(code, label, 0.0, 0, "", HeadcountTargetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountTargetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountTargetStatus next)
    {
        switch (Status)
        {
            case HeadcountTargetStatus.Proposed:
                switch (next)
                {
                    case HeadcountTargetStatus.Approved:
                    case HeadcountTargetStatus.Withdrawn:
                        return true;
                }
                return false;
            case HeadcountTargetStatus.Approved:
                return next == HeadcountTargetStatus.Rejected;
            case HeadcountTargetStatus.Rejected:
                return next == HeadcountTargetStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(HeadcountTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"headcount target"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Cost - (double)Retries, 2);
    }

    public HeadcountTarget Copy()
    {
        return new HeadcountTarget(Code, Label, Cost, Retries, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
