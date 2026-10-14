namespace Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pulse survey override is in its life cycle.</summary>
public enum StrictPulseSurveyOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict pulse survey override as the pulse survey team keeps it.</summary>
public sealed class StrictPulseSurveyOverride(string code, string label, double discount, int endsAt, string phone, string checksum, int updatedAt, bool approved, string department, StrictPulseSurveyOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public int EndsAt { get; set; } = endsAt;
    public string Phone { get; set; } = phone;
    public string Checksum { get; set; } = checksum;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Approved { get; set; } = approved;
    public string Department { get; set; } = department;
    public StrictPulseSurveyOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a strict pulse survey override in status proposed.</summary>
    public static StrictPulseSurveyOverride NewStrictPulseSurveyOverride(string code, string label)
    {
        return new StrictPulseSurveyOverride(
            code,
            label,
            0.0,
            0,
            "",
            "",
            0,
            false,
            "",
            StrictPulseSurveyOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPulseSurveyOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPulseSurveyOverrideStatus next)
    {
        switch (Status)
        {
            case StrictPulseSurveyOverrideStatus.Proposed:
                return next == StrictPulseSurveyOverrideStatus.Approved || next == StrictPulseSurveyOverrideStatus.Rejected;
            case StrictPulseSurveyOverrideStatus.Approved:
                switch (next)
                {
                    case StrictPulseSurveyOverrideStatus.Rejected:
                    case StrictPulseSurveyOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictPulseSurveyOverrideStatus.Rejected:
                switch (next)
                {
                    case StrictPulseSurveyOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictPulseSurveyOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pulse survey override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)UpdatedAt * (double)EndsAt, 2);
    }

    public StrictPulseSurveyOverride Copy()
    {
        return new StrictPulseSurveyOverride(
            Code,
            Label,
            Discount,
            EndsAt,
            Phone,
            Checksum,
            UpdatedAt,
            Approved,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
