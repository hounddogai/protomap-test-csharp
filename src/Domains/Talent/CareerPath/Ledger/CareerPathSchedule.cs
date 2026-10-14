namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path schedule is in its life cycle.</summary>
public enum CareerPathScheduleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One career path schedule, with what the planners need to act on it.</summary>
public sealed class CareerPathSchedule(string code, string label, string segment, int createdAt, int durationMinutes, string contactEmail, string unit, string costCenter, int revision, CareerPathScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public int CreatedAt { get; set; } = createdAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ContactEmail { get; set; } = contactEmail;
    public string Unit { get; set; } = unit;
    public string CostCenter { get; set; } = costCenter;
    public int Revision { get; set; } = revision;
    public CareerPathScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a career path schedule in status idle.</summary>
    public static CareerPathSchedule NewCareerPathSchedule(string code, string label)
    {
        return new CareerPathSchedule(code, label, "", 0, 0, "", "", "", 0, CareerPathScheduleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathScheduleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathScheduleStatus next)
    {
        switch (Status)
        {
            case CareerPathScheduleStatus.Idle:
                switch (next)
                {
                    case CareerPathScheduleStatus.Ready:
                    case CareerPathScheduleStatus.Draining:
                        return true;
                }
                return false;
            case CareerPathScheduleStatus.Warming:
                return next == CareerPathScheduleStatus.Stopped;
            case CareerPathScheduleStatus.Ready:
                switch (next)
                {
                    case CareerPathScheduleStatus.Stopped:
                        return true;
                }
                return false;
            case CareerPathScheduleStatus.Draining:
                switch (next)
                {
                    case CareerPathScheduleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"career path schedule"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)CreatedAt * (double)Revision, 2);
    }

    public CareerPathSchedule Copy()
    {
        return new CareerPathSchedule(
            Code,
            Label,
            Segment,
            CreatedAt,
            DurationMinutes,
            ContactEmail,
            Unit,
            CostCenter,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
