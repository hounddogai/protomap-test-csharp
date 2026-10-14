namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary accrual schedule is in its life cycle.</summary>
public enum PrimaryAccrualScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a primary accrual schedule and its current status.</summary>
public sealed class PrimaryAccrualSchedule(string code, string label, string title, string name, string channel, string source, double budget, double discount, string salaryBand, string badge, double rating, PrimaryAccrualScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public string Source { get; set; } = source;
    public double Budget { get; set; } = budget;
    public double Discount { get; set; } = discount;
    public string SalaryBand { get; set; } = salaryBand;
    public string Badge { get; set; } = badge;
    public double Rating { get; set; } = rating;
    public PrimaryAccrualScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a primary accrual schedule in status planned.</summary>
    public static PrimaryAccrualSchedule NewPrimaryAccrualSchedule(string code, string label)
    {
        return new PrimaryAccrualSchedule(
            code,
            label,
            "",
            "",
            "",
            "",
            0.0,
            0.0,
            "",
            "",
            0.0,
            PrimaryAccrualScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAccrualScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAccrualScheduleStatus next)
    {
        switch (Status)
        {
            case PrimaryAccrualScheduleStatus.Planned:
                return next == PrimaryAccrualScheduleStatus.Scheduled || next == PrimaryAccrualScheduleStatus.Completed;
            case PrimaryAccrualScheduleStatus.Scheduled:
                switch (next)
                {
                    case PrimaryAccrualScheduleStatus.Completed:
                        return true;
                }
                return false;
            case PrimaryAccrualScheduleStatus.InProgress:
                return next == PrimaryAccrualScheduleStatus.Completed || next == PrimaryAccrualScheduleStatus.Cancelled;
            case PrimaryAccrualScheduleStatus.Completed:
                switch (next)
                {
                    case PrimaryAccrualScheduleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAccrualScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary accrual schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"primary accrual schedule"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Budget - Discount, 2);
    }

    public PrimaryAccrualSchedule Copy()
    {
        return new PrimaryAccrualSchedule(
            Code,
            Label,
            Title,
            Name,
            Channel,
            Source,
            Budget,
            Discount,
            SalaryBand,
            Badge,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
