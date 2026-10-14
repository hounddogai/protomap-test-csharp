namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted garnishment milestone is in its life cycle.</summary>
public enum WeightedGarnishmentMilestoneStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A weighted garnishment milestone as the garnishment team keeps it.</summary>
public sealed class WeightedGarnishmentMilestone(string code, string title, bool archived, string segment, string salaryBand, int quantity, WeightedGarnishmentMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Segment { get; set; } = segment;
    public string SalaryBand { get; set; } = salaryBand;
    public int Quantity { get; set; } = quantity;
    public WeightedGarnishmentMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a weighted garnishment milestone in status new.</summary>
    public static WeightedGarnishmentMilestone NewWeightedGarnishmentMilestone(string code, string title)
    {
        return new WeightedGarnishmentMilestone(
            code,
            title,
            false,
            "",
            "",
            0,
            WeightedGarnishmentMilestoneStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedGarnishmentMilestoneStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedGarnishmentMilestoneStatus next)
    {
        switch (Status)
        {
            case WeightedGarnishmentMilestoneStatus.New:
                return next == WeightedGarnishmentMilestoneStatus.Failed;
            case WeightedGarnishmentMilestoneStatus.Queued:
                return next == WeightedGarnishmentMilestoneStatus.Done || next == WeightedGarnishmentMilestoneStatus.Failed;
            case WeightedGarnishmentMilestoneStatus.Running:
                switch (next)
                {
                    case WeightedGarnishmentMilestoneStatus.Done:
                        return true;
                }
                return false;
            case WeightedGarnishmentMilestoneStatus.Done:
                switch (next)
                {
                    case WeightedGarnishmentMilestoneStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedGarnishmentMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted garnishment milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public WeightedGarnishmentMilestone Copy()
    {
        return new WeightedGarnishmentMilestone(Code, Title, Archived, Segment, SalaryBand, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
