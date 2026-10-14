namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled booking template is in its life cycle.</summary>
public enum ScheduledBookingTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One scheduled booking template, with what the planners need to act on it.</summary>
public sealed class ScheduledBookingTemplate(string code, string name, string location, int quantity, string grade, string title, double score, bool archived, ScheduledBookingTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public int Quantity { get; set; } = quantity;
    public string Grade { get; set; } = grade;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public ScheduledBookingTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled booking template in status proposed.</summary>
    public static ScheduledBookingTemplate NewScheduledBookingTemplate(string code, string name)
    {
        return new ScheduledBookingTemplate(
            code,
            name,
            "",
            0,
            "",
            "",
            0.0,
            false,
            ScheduledBookingTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBookingTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBookingTemplateStatus next)
    {
        switch (Status)
        {
            case ScheduledBookingTemplateStatus.Proposed:
                switch (next)
                {
                    case ScheduledBookingTemplateStatus.Approved:
                    case ScheduledBookingTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledBookingTemplateStatus.Approved:
                return next == ScheduledBookingTemplateStatus.Withdrawn;
            case ScheduledBookingTemplateStatus.Rejected:
                return next == ScheduledBookingTemplateStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScheduledBookingTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled booking template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled booking template"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Quantity - Score, 2);
    }

    public ScheduledBookingTemplate Copy()
    {
        return new ScheduledBookingTemplate(Code, Name, Location, Quantity, Grade, Title, Score, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
