namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey assignment is in its life cycle.</summary>
public enum PulseSurveyAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pulse survey assignment and its current status.</summary>
public sealed class PulseSurveyAssignment(string code, string name, string phone, string country, string locale, bool visible, string skill, string title, double amount, PulseSurveyAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public bool Visible { get; set; } = visible;
    public string Skill { get; set; } = skill;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public PulseSurveyAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey assignment in status idle.</summary>
    public static PulseSurveyAssignment NewPulseSurveyAssignment(string code, string name)
    {
        return new PulseSurveyAssignment(
            code,
            name,
            "",
            "",
            "",
            false,
            "",
            "",
            0.0,
            PulseSurveyAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyAssignmentStatus next)
    {
        switch (Status)
        {
            case PulseSurveyAssignmentStatus.Idle:
                switch (next)
                {
                    case PulseSurveyAssignmentStatus.Ready:
                        return true;
                }
                return false;
            case PulseSurveyAssignmentStatus.Warming:
                return next == PulseSurveyAssignmentStatus.Stopped;
            case PulseSurveyAssignmentStatus.Ready:
                return next == PulseSurveyAssignmentStatus.Draining || next == PulseSurveyAssignmentStatus.Stopped;
            case PulseSurveyAssignmentStatus.Draining:
                switch (next)
                {
                    case PulseSurveyAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PulseSurveyAssignment Copy()
    {
        return new PulseSurveyAssignment(Code, Name, Phone, Country, Locale, Visible, Skill, Title, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
