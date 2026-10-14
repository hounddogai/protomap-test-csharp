namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pulse survey forecast is in its life cycle.</summary>
public enum CappedPulseSurveyForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A capped pulse survey forecast. Its code is unique within a tenant.</summary>
public sealed class CappedPulseSurveyForecast(string code, string title, int capacity, int expiresAt, string grade, string phone, Dictionary<string, string> attributes, CappedPulseSurveyForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Grade { get; set; } = grade;
    public string Phone { get; set; } = phone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public CappedPulseSurveyForecastStatus Status { get; set; } = status;

    /// <summary>Starts a capped pulse survey forecast in status proposed.</summary>
    public static CappedPulseSurveyForecast NewCappedPulseSurveyForecast(string code, string title)
    {
        return new CappedPulseSurveyForecast(
            code,
            title,
            0,
            0,
            "",
            "",
            new Dictionary<string, string>(),
            CappedPulseSurveyForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPulseSurveyForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPulseSurveyForecastStatus next)
    {
        switch (Status)
        {
            case CappedPulseSurveyForecastStatus.Proposed:
                return next == CappedPulseSurveyForecastStatus.Approved || next == CappedPulseSurveyForecastStatus.Rejected;
            case CappedPulseSurveyForecastStatus.Approved:
                switch (next)
                {
                    case CappedPulseSurveyForecastStatus.Rejected:
                    case CappedPulseSurveyForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedPulseSurveyForecastStatus.Rejected:
                switch (next)
                {
                    case CappedPulseSurveyForecastStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedPulseSurveyForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pulse survey forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)ExpiresAt * (double)Capacity, 2);
    }

    public CappedPulseSurveyForecast Copy()
    {
        return new CappedPulseSurveyForecast(Code, Title, Capacity, ExpiresAt, Grade, Phone, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
