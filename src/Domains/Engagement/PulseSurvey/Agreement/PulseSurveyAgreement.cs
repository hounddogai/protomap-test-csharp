namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey agreement is in its life cycle.</summary>
public enum PulseSurveyAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pulse survey agreement as the pulse survey team keeps it.</summary>
public sealed class PulseSurveyAgreement(string code, string title, string timezone, string managerId, double amount, string category, int attempts, bool pinned, double maximum, PulseSurveyAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public string ManagerId { get; set; } = managerId;
    public double Amount { get; set; } = amount;
    public string Category { get; set; } = category;
    public int Attempts { get; set; } = attempts;
    public bool Pinned { get; set; } = pinned;
    public double Maximum { get; set; } = maximum;
    public PulseSurveyAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey agreement in status planned.</summary>
    public static PulseSurveyAgreement NewPulseSurveyAgreement(string code, string title)
    {
        return new PulseSurveyAgreement(
            code,
            title,
            "",
            "",
            0.0,
            "",
            0,
            false,
            0.0,
            PulseSurveyAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyAgreementStatus next)
    {
        switch (Status)
        {
            case PulseSurveyAgreementStatus.Planned:
                return next == PulseSurveyAgreementStatus.Scheduled;
            case PulseSurveyAgreementStatus.Scheduled:
                return next == PulseSurveyAgreementStatus.Completed;
            case PulseSurveyAgreementStatus.InProgress:
                switch (next)
                {
                    case PulseSurveyAgreementStatus.Completed:
                    case PulseSurveyAgreementStatus.Cancelled:
                        return true;
                }
                return false;
            case PulseSurveyAgreementStatus.Completed:
                switch (next)
                {
                    case PulseSurveyAgreementStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Attempts + Maximum, 2);
    }

    public PulseSurveyAgreement Copy()
    {
        return new PulseSurveyAgreement(
            Code,
            Title,
            Timezone,
            ManagerId,
            Amount,
            Category,
            Attempts,
            Pinned,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
