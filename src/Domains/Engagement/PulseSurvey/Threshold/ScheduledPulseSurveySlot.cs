namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pulse survey slot is in its life cycle.</summary>
public enum ScheduledPulseSurveySlotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scheduled pulse survey slot. Its code is unique within a tenant.</summary>
public sealed class ScheduledPulseSurveySlot(string code, string title, double ratio, double hours, string costCenter, string name, int durationMinutes, double maximum, ScheduledPulseSurveySlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public double Hours { get; set; } = hours;
    public string CostCenter { get; set; } = costCenter;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Maximum { get; set; } = maximum;
    public ScheduledPulseSurveySlotStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pulse survey slot in status created.</summary>
    public static ScheduledPulseSurveySlot NewScheduledPulseSurveySlot(string code, string title)
    {
        return new ScheduledPulseSurveySlot(
            code,
            title,
            0.0,
            0.0,
            "",
            "",
            0,
            0.0,
            ScheduledPulseSurveySlotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPulseSurveySlotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPulseSurveySlotStatus next)
    {
        switch (Status)
        {
            case ScheduledPulseSurveySlotStatus.Created:
                switch (next)
                {
                    case ScheduledPulseSurveySlotStatus.Validated:
                    case ScheduledPulseSurveySlotStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledPulseSurveySlotStatus.Validated:
                return next == ScheduledPulseSurveySlotStatus.Published || next == ScheduledPulseSurveySlotStatus.Retired;
            case ScheduledPulseSurveySlotStatus.Published:
                switch (next)
                {
                    case ScheduledPulseSurveySlotStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledPulseSurveySlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pulse survey slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Maximum - Ratio, 2);
    }

    public ScheduledPulseSurveySlot Copy()
    {
        return new ScheduledPulseSurveySlot(
            Code,
            Title,
            Ratio,
            Hours,
            CostCenter,
            Name,
            DurationMinutes,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
