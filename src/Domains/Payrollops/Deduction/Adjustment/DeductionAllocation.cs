namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction allocation is in its life cycle.</summary>
public enum DeductionAllocationStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A deduction allocation. Its code is unique within a tenant.</summary>
public sealed class DeductionAllocation(string code, string title, int createdAt, int startsAt, double rating, string salaryBand, int revision, string description, DeductionAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public int StartsAt { get; set; } = startsAt;
    public double Rating { get; set; } = rating;
    public string SalaryBand { get; set; } = salaryBand;
    public int Revision { get; set; } = revision;
    public string Description { get; set; } = description;
    public DeductionAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a deduction allocation in status new.</summary>
    public static DeductionAllocation NewDeductionAllocation(string code, string title)
    {
        return new DeductionAllocation(code, title, 0, 0, 0.0, "", 0, "", DeductionAllocationStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionAllocationStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionAllocationStatus next)
    {
        switch (Status)
        {
            case DeductionAllocationStatus.New:
                return next == DeductionAllocationStatus.Queued || next == DeductionAllocationStatus.Done;
            case DeductionAllocationStatus.Queued:
                switch (next)
                {
                    case DeductionAllocationStatus.Done:
                    case DeductionAllocationStatus.Failed:
                        return true;
                }
                return false;
            case DeductionAllocationStatus.Running:
                switch (next)
                {
                    case DeductionAllocationStatus.Done:
                    case DeductionAllocationStatus.Failed:
                        return true;
                }
                return false;
            case DeductionAllocationStatus.Done:
                return next == DeductionAllocationStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DeductionAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"deduction allocation"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)StartsAt - Rating, 2);
    }

    public DeductionAllocation Copy()
    {
        return new DeductionAllocation(
            Code,
            Title,
            CreatedAt,
            StartsAt,
            Rating,
            SalaryBand,
            Revision,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
