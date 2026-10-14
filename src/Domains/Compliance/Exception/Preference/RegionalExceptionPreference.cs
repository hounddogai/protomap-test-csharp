namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional exception preference is in its life cycle.</summary>
public enum RegionalExceptionPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a regional exception preference and its current status.</summary>
public sealed class RegionalExceptionPreference(string code, string label, string badge, string locale, int durationMinutes, int expiresAt, RegionalExceptionPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Locale { get; set; } = locale;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int ExpiresAt { get; set; } = expiresAt;
    public RegionalExceptionPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a regional exception preference in status created.</summary>
    public static RegionalExceptionPreference NewRegionalExceptionPreference(string code, string label)
    {
        return new RegionalExceptionPreference(code, label, "", "", 0, 0, RegionalExceptionPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalExceptionPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalExceptionPreferenceStatus next)
    {
        switch (Status)
        {
            case RegionalExceptionPreferenceStatus.Created:
                switch (next)
                {
                    case RegionalExceptionPreferenceStatus.Validated:
                        return true;
                }
                return false;
            case RegionalExceptionPreferenceStatus.Validated:
                switch (next)
                {
                    case RegionalExceptionPreferenceStatus.Published:
                        return true;
                }
                return false;
            case RegionalExceptionPreferenceStatus.Published:
                switch (next)
                {
                    case RegionalExceptionPreferenceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalExceptionPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional exception preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)ExpiresAt * (double)DurationMinutes, 2);
    }

    public RegionalExceptionPreference Copy()
    {
        return new RegionalExceptionPreference(Code, Label, Badge, Locale, DurationMinutes, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
