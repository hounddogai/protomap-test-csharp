namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey notice is in its life cycle.</summary>
public enum PulseSurveyNoticeStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pulse survey notice. Its code is unique within a tenant.</summary>
public sealed class PulseSurveyNotice(string code, string title, string unit, string description, int quantity, PulseSurveyNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public int Quantity { get; set; } = quantity;
    public PulseSurveyNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey notice in status planned.</summary>
    public static PulseSurveyNotice NewPulseSurveyNotice(string code, string title)
    {
        return new PulseSurveyNotice(code, title, "", "", 0, PulseSurveyNoticeStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyNoticeStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyNoticeStatus next)
    {
        switch (Status)
        {
            case PulseSurveyNoticeStatus.Planned:
                switch (next)
                {
                    case PulseSurveyNoticeStatus.Cancelled:
                        return true;
                }
                return false;
            case PulseSurveyNoticeStatus.Scheduled:
                return next == PulseSurveyNoticeStatus.Completed;
            case PulseSurveyNoticeStatus.InProgress:
                switch (next)
                {
                    case PulseSurveyNoticeStatus.Completed:
                        return true;
                }
                return false;
            case PulseSurveyNoticeStatus.Completed:
                switch (next)
                {
                    case PulseSurveyNoticeStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public PulseSurveyNotice Copy()
    {
        return new PulseSurveyNotice(Code, Title, Unit, Description, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
