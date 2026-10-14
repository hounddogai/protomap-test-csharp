namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount schedule is in its life cycle.</summary>
public enum HeadcountScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a headcount schedule and its current status.</summary>
public sealed class HeadcountSchedule(string code, string label, string unit, string region, string currency, string ownerId, double ratio, double discount, HeadcountScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Region { get; set; } = region;
    public string Currency { get; set; } = currency;
    public string OwnerId { get; set; } = ownerId;
    public double Ratio { get; set; } = ratio;
    public double Discount { get; set; } = discount;
    public HeadcountScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a headcount schedule in status proposed.</summary>
    public static HeadcountSchedule NewHeadcountSchedule(string code, string label)
    {
        return new HeadcountSchedule(code, label, "", "", "", "", 0.0, 0.0, HeadcountScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountScheduleStatus next)
    {
        switch (Status)
        {
            case HeadcountScheduleStatus.Proposed:
                switch (next)
                {
                    case HeadcountScheduleStatus.Approved:
                    case HeadcountScheduleStatus.Rejected:
                        return true;
                }
                return false;
            case HeadcountScheduleStatus.Approved:
                switch (next)
                {
                    case HeadcountScheduleStatus.Rejected:
                    case HeadcountScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case HeadcountScheduleStatus.Rejected:
                switch (next)
                {
                    case HeadcountScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ratio + Discount, 2);
    }

    public HeadcountSchedule Copy()
    {
        return new HeadcountSchedule(Code, Label, Unit, Region, Currency, OwnerId, Ratio, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
