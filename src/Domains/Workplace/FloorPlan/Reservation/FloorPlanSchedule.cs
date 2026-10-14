namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan schedule is in its life cycle.</summary>
public enum FloorPlanScheduleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A floor plan schedule as the floor plan team keeps it.</summary>
public sealed class FloorPlanSchedule(string code, string name, int windowDays, bool visible, double floorValue, int expiresAt, string phone, int retries, double fte, string salaryBand, string channel, FloorPlanScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public bool Visible { get; set; } = visible;
    public double FloorValue { get; set; } = floorValue;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Phone { get; set; } = phone;
    public int Retries { get; set; } = retries;
    public double Fte { get; set; } = fte;
    public string SalaryBand { get; set; } = salaryBand;
    public string Channel { get; set; } = channel;
    public FloorPlanScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan schedule in status open.</summary>
    public static FloorPlanSchedule NewFloorPlanSchedule(string code, string name)
    {
        return new FloorPlanSchedule(code, name, 0, false, 0.0, 0, "", 0, 0.0, "", "", FloorPlanScheduleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanScheduleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanScheduleStatus next)
    {
        switch (Status)
        {
            case FloorPlanScheduleStatus.Open:
                return next == FloorPlanScheduleStatus.InReview;
            case FloorPlanScheduleStatus.InReview:
                return next == FloorPlanScheduleStatus.Resolved;
            case FloorPlanScheduleStatus.Resolved:
                switch (next)
                {
                    case FloorPlanScheduleStatus.Reopened:
                        return true;
                }
                return false;
            case FloorPlanScheduleStatus.Reopened:
                switch (next)
                {
                    case FloorPlanScheduleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)ExpiresAt - FloorValue, 2);
    }

    public FloorPlanSchedule Copy()
    {
        return new FloorPlanSchedule(
            Code,
            Name,
            WindowDays,
            Visible,
            FloorValue,
            ExpiresAt,
            Phone,
            Retries,
            Fte,
            SalaryBand,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
