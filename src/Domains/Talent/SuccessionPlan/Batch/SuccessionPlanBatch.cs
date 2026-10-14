namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan batch is in its life cycle.</summary>
public enum SuccessionPlanBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a succession plan batch and its current status.</summary>
public sealed class SuccessionPlanBatch(string code, string title, string region, string channel, string grade, int priority, string phone, bool active, SuccessionPlanBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string Channel { get; set; } = channel;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public string Phone { get; set; } = phone;
    public bool Active { get; set; } = active;
    public SuccessionPlanBatchStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan batch in status proposed.</summary>
    public static SuccessionPlanBatch NewSuccessionPlanBatch(string code, string title)
    {
        return new SuccessionPlanBatch(code, title, "", "", "", 0, "", false, SuccessionPlanBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanBatchStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanBatchStatus.Proposed:
                return next == SuccessionPlanBatchStatus.Approved;
            case SuccessionPlanBatchStatus.Approved:
                return next == SuccessionPlanBatchStatus.Rejected;
            case SuccessionPlanBatchStatus.Rejected:
                return next == SuccessionPlanBatchStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public SuccessionPlanBatch Copy()
    {
        return new SuccessionPlanBatch(Code, Title, Region, Channel, Grade, Priority, Phone, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
