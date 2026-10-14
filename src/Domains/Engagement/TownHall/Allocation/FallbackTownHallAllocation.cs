namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback town hall allocation is in its life cycle.</summary>
public enum FallbackTownHallAllocationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One fallback town hall allocation, with what the planners need to act on it.</summary>
public sealed class FallbackTownHallAllocation(string code, string title, double minimum, string badge, double rating, string timezone, string salaryBand, int attempts, string employeeId, FallbackTownHallAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Badge { get; set; } = badge;
    public double Rating { get; set; } = rating;
    public string Timezone { get; set; } = timezone;
    public string SalaryBand { get; set; } = salaryBand;
    public int Attempts { get; set; } = attempts;
    public string EmployeeId { get; set; } = employeeId;
    public FallbackTownHallAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a fallback town hall allocation in status new.</summary>
    public static FallbackTownHallAllocation NewFallbackTownHallAllocation(string code, string title)
    {
        return new FallbackTownHallAllocation(
            code,
            title,
            0.0,
            "",
            0.0,
            "",
            "",
            0,
            "",
            FallbackTownHallAllocationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackTownHallAllocationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackTownHallAllocationStatus next)
    {
        switch (Status)
        {
            case FallbackTownHallAllocationStatus.New:
                switch (next)
                {
                    case FallbackTownHallAllocationStatus.Running:
                    case FallbackTownHallAllocationStatus.Failed:
                        return true;
                }
                return false;
            case FallbackTownHallAllocationStatus.Queued:
                return next == FallbackTownHallAllocationStatus.Running || next == FallbackTownHallAllocationStatus.Failed;
            case FallbackTownHallAllocationStatus.Running:
                return next == FallbackTownHallAllocationStatus.Done || next == FallbackTownHallAllocationStatus.Failed;
            case FallbackTownHallAllocationStatus.Done:
                return next == FallbackTownHallAllocationStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FallbackTownHallAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback town hall allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback town hall allocation"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Attempts - Rating, 2);
    }

    public FallbackTownHallAllocation Copy()
    {
        return new FallbackTownHallAllocation(
            Code,
            Title,
            Minimum,
            Badge,
            Rating,
            Timezone,
            SalaryBand,
            Attempts,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
