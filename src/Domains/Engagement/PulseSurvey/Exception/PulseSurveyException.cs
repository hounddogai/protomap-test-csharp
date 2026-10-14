namespace Acme.Workforce.Engagement.PulseSurvey.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey exception is in its life cycle.</summary>
public enum PulseSurveyExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pulse survey exception, with what the planners need to act on it.</summary>
public sealed class PulseSurveyException(string code, string title, double budget, int retries, string badge, int endsAt, string externalRef, double weight, string category, string region, PulseSurveyExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public int Retries { get; set; } = retries;
    public string Badge { get; set; } = badge;
    public int EndsAt { get; set; } = endsAt;
    public string ExternalRef { get; set; } = externalRef;
    public double Weight { get; set; } = weight;
    public string Category { get; set; } = category;
    public string Region { get; set; } = region;
    public PulseSurveyExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey exception in status requested.</summary>
    public static PulseSurveyException NewPulseSurveyException(string code, string title)
    {
        return new PulseSurveyException(
            code,
            title,
            0.0,
            0,
            "",
            0,
            "",
            0.0,
            "",
            "",
            PulseSurveyExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyExceptionStatus next)
    {
        switch (Status)
        {
            case PulseSurveyExceptionStatus.Requested:
                return next == PulseSurveyExceptionStatus.Granted;
            case PulseSurveyExceptionStatus.Granted:
                return next == PulseSurveyExceptionStatus.Expired;
            case PulseSurveyExceptionStatus.Revoked:
                switch (next)
                {
                    case PulseSurveyExceptionStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Retries * (double)EndsAt, 2);
    }

    public PulseSurveyException Copy()
    {
        return new PulseSurveyException(
            Code,
            Title,
            Budget,
            Retries,
            Badge,
            EndsAt,
            ExternalRef,
            Weight,
            Category,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
