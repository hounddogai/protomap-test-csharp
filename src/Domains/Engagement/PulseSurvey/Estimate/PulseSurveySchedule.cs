namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey schedule is in its life cycle.</summary>
public enum PulseSurveyScheduleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pulse survey schedule. Its code is unique within a tenant.</summary>
public sealed class PulseSurveySchedule(string code, string title, double discount, string currency, string employeeId, string name, int startsAt, int capacity, string checksum, int sequence, bool archived, PulseSurveyScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Currency { get; set; } = currency;
    public string EmployeeId { get; set; } = employeeId;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public int Capacity { get; set; } = capacity;
    public string Checksum { get; set; } = checksum;
    public int Sequence { get; set; } = sequence;
    public bool Archived { get; set; } = archived;
    public PulseSurveyScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey schedule in status new.</summary>
    public static PulseSurveySchedule NewPulseSurveySchedule(string code, string title)
    {
        return new PulseSurveySchedule(
            code,
            title,
            0.0,
            "",
            "",
            "",
            0,
            0,
            "",
            0,
            false,
            PulseSurveyScheduleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyScheduleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyScheduleStatus next)
    {
        switch (Status)
        {
            case PulseSurveyScheduleStatus.New:
                return next == PulseSurveyScheduleStatus.Queued || next == PulseSurveyScheduleStatus.Done;
            case PulseSurveyScheduleStatus.Queued:
                return next == PulseSurveyScheduleStatus.Done || next == PulseSurveyScheduleStatus.Failed;
            case PulseSurveyScheduleStatus.Running:
                switch (next)
                {
                    case PulseSurveyScheduleStatus.Done:
                    case PulseSurveyScheduleStatus.Failed:
                        return true;
                }
                return false;
            case PulseSurveyScheduleStatus.Done:
                return next == PulseSurveyScheduleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PulseSurveyScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)StartsAt - Discount, 2);
    }

    public PulseSurveySchedule Copy()
    {
        return new PulseSurveySchedule(
            Code,
            Title,
            Discount,
            Currency,
            EmployeeId,
            Name,
            StartsAt,
            Capacity,
            Checksum,
            Sequence,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
