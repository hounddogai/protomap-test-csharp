namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback recognition preference is in its life cycle.</summary>
public enum FallbackRecognitionPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback recognition preference as the recognition team keeps it.</summary>
public sealed class FallbackRecognitionPreference(string code, string name, int windowDays, int startsAt, string country, string locale, string phone, FallbackRecognitionPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int StartsAt { get; set; } = startsAt;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public string Phone { get; set; } = phone;
    public FallbackRecognitionPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a fallback recognition preference in status new.</summary>
    public static FallbackRecognitionPreference NewFallbackRecognitionPreference(string code, string name)
    {
        return new FallbackRecognitionPreference(
            code,
            name,
            0,
            0,
            "",
            "",
            "",
            FallbackRecognitionPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRecognitionPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRecognitionPreferenceStatus next)
    {
        switch (Status)
        {
            case FallbackRecognitionPreferenceStatus.New:
                return next == FallbackRecognitionPreferenceStatus.Done;
            case FallbackRecognitionPreferenceStatus.Queued:
                return next == FallbackRecognitionPreferenceStatus.Done;
            case FallbackRecognitionPreferenceStatus.Running:
                return next == FallbackRecognitionPreferenceStatus.Done || next == FallbackRecognitionPreferenceStatus.Failed;
            case FallbackRecognitionPreferenceStatus.Done:
                return next == FallbackRecognitionPreferenceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FallbackRecognitionPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback recognition preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)StartsAt * (double)WindowDays, 2);
    }

    public FallbackRecognitionPreference Copy()
    {
        return new FallbackRecognitionPreference(Code, Name, WindowDays, StartsAt, Country, Locale, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
