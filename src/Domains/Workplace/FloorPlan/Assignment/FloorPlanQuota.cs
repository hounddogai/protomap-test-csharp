namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan quota is in its life cycle.</summary>
public enum FloorPlanQuotaStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One floor plan quota, with what the planners need to act on it.</summary>
public sealed class FloorPlanQuota(string code, string name, int sequence, int tenureMonths, string currency, string grade, string managerId, FloorPlanQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Currency { get; set; } = currency;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public FloorPlanQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan quota in status new.</summary>
    public static FloorPlanQuota NewFloorPlanQuota(string code, string name)
    {
        return new FloorPlanQuota(code, name, 0, 0, "", "", "", FloorPlanQuotaStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanQuotaStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanQuotaStatus next)
    {
        switch (Status)
        {
            case FloorPlanQuotaStatus.New:
                switch (next)
                {
                    case FloorPlanQuotaStatus.Queued:
                    case FloorPlanQuotaStatus.Failed:
                        return true;
                }
                return false;
            case FloorPlanQuotaStatus.Queued:
                switch (next)
                {
                    case FloorPlanQuotaStatus.Done:
                        return true;
                }
                return false;
            case FloorPlanQuotaStatus.Running:
                return next == FloorPlanQuotaStatus.Done || next == FloorPlanQuotaStatus.Failed;
            case FloorPlanQuotaStatus.Done:
                switch (next)
                {
                    case FloorPlanQuotaStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Sequence + (double)TenureMonths, 2);
    }

    public FloorPlanQuota Copy()
    {
        return new FloorPlanQuota(Code, Name, Sequence, TenureMonths, Currency, Grade, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
