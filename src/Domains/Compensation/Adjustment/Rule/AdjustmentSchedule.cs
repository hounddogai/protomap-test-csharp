namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment schedule is in its life cycle.</summary>
public enum AdjustmentScheduleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a adjustment schedule and its current status.</summary>
public sealed class AdjustmentSchedule(string code, string label, int sequence, string summary, int graceDays, string managerId, double discount, string jobCode, string region, string category, string reviewer, AdjustmentScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public string Summary { get; set; } = summary;
    public int GraceDays { get; set; } = graceDays;
    public string ManagerId { get; set; } = managerId;
    public double Discount { get; set; } = discount;
    public string JobCode { get; set; } = jobCode;
    public string Region { get; set; } = region;
    public string Category { get; set; } = category;
    public string Reviewer { get; set; } = reviewer;
    public AdjustmentScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment schedule in status created.</summary>
    public static AdjustmentSchedule NewAdjustmentSchedule(string code, string label)
    {
        return new AdjustmentSchedule(
            code,
            label,
            0,
            "",
            0,
            "",
            0.0,
            "",
            "",
            "",
            "",
            AdjustmentScheduleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentScheduleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentScheduleStatus next)
    {
        switch (Status)
        {
            case AdjustmentScheduleStatus.Created:
                return next == AdjustmentScheduleStatus.Retired;
            case AdjustmentScheduleStatus.Validated:
                return next == AdjustmentScheduleStatus.Published || next == AdjustmentScheduleStatus.Retired;
            case AdjustmentScheduleStatus.Published:
                return next == AdjustmentScheduleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdjustmentScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Sequence - (double)GraceDays, 2);
    }

    public AdjustmentSchedule Copy()
    {
        return new AdjustmentSchedule(
            Code,
            Label,
            Sequence,
            Summary,
            GraceDays,
            ManagerId,
            Discount,
            JobCode,
            Region,
            Category,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
