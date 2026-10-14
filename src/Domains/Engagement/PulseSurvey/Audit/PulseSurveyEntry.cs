namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey entry is in its life cycle.</summary>
public enum PulseSurveyEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pulse survey entry as the pulse survey team keeps it.</summary>
public sealed class PulseSurveyEntry(string code, string title, int attempts, bool archived, string reviewer, string salaryBand, PulseSurveyEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public bool Archived { get; set; } = archived;
    public string Reviewer { get; set; } = reviewer;
    public string SalaryBand { get; set; } = salaryBand;
    public PulseSurveyEntryStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey entry in status planned.</summary>
    public static PulseSurveyEntry NewPulseSurveyEntry(string code, string title)
    {
        return new PulseSurveyEntry(code, title, 0, false, "", "", PulseSurveyEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyEntryStatus next)
    {
        switch (Status)
        {
            case PulseSurveyEntryStatus.Planned:
                return next == PulseSurveyEntryStatus.InProgress;
            case PulseSurveyEntryStatus.Scheduled:
                return next == PulseSurveyEntryStatus.Completed;
            case PulseSurveyEntryStatus.InProgress:
                switch (next)
                {
                    case PulseSurveyEntryStatus.Completed:
                    case PulseSurveyEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case PulseSurveyEntryStatus.Completed:
                switch (next)
                {
                    case PulseSurveyEntryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public PulseSurveyEntry Copy()
    {
        return new PulseSurveyEntry(Code, Title, Attempts, Archived, Reviewer, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
