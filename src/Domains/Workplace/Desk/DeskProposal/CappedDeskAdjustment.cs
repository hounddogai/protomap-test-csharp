namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped desk adjustment is in its life cycle.</summary>
public enum CappedDeskAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a capped desk adjustment and its current status.</summary>
public sealed class CappedDeskAdjustment(string code, string label, double threshold, int durationMinutes, string description, double ratio, string badge, double budget, string salaryBand, int attempts, string category, CappedDeskAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Description { get; set; } = description;
    public double Ratio { get; set; } = ratio;
    public string Badge { get; set; } = badge;
    public double Budget { get; set; } = budget;
    public string SalaryBand { get; set; } = salaryBand;
    public int Attempts { get; set; } = attempts;
    public string Category { get; set; } = category;
    public CappedDeskAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a capped desk adjustment in status new.</summary>
    public static CappedDeskAdjustment NewCappedDeskAdjustment(string code, string label)
    {
        return new CappedDeskAdjustment(
            code,
            label,
            0.0,
            0,
            "",
            0.0,
            "",
            0.0,
            "",
            0,
            "",
            CappedDeskAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedDeskAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedDeskAdjustmentStatus next)
    {
        switch (Status)
        {
            case CappedDeskAdjustmentStatus.New:
                return next == CappedDeskAdjustmentStatus.Queued || next == CappedDeskAdjustmentStatus.Failed;
            case CappedDeskAdjustmentStatus.Queued:
                switch (next)
                {
                    case CappedDeskAdjustmentStatus.Running:
                    case CappedDeskAdjustmentStatus.Done:
                        return true;
                }
                return false;
            case CappedDeskAdjustmentStatus.Running:
                switch (next)
                {
                    case CappedDeskAdjustmentStatus.Done:
                    case CappedDeskAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case CappedDeskAdjustmentStatus.Done:
                switch (next)
                {
                    case CappedDeskAdjustmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedDeskAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped desk adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"capped desk adjustment"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)DurationMinutes * (double)Attempts, 2);
    }

    public CappedDeskAdjustment Copy()
    {
        return new CappedDeskAdjustment(
            Code,
            Label,
            Threshold,
            DurationMinutes,
            Description,
            Ratio,
            Badge,
            Budget,
            SalaryBand,
            Attempts,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
