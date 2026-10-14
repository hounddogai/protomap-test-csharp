namespace Acme.Workforce.Engagement.Recognition.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient recognition preference is in its life cycle.</summary>
public enum LenientRecognitionPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A lenient recognition preference as the recognition team keeps it.</summary>
public sealed class LenientRecognitionPreference(string code, string title, string summary, string employeeId, double fte, int attempts, LenientRecognitionPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string EmployeeId { get; set; } = employeeId;
    public double Fte { get; set; } = fte;
    public int Attempts { get; set; } = attempts;
    public LenientRecognitionPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a lenient recognition preference in status open.</summary>
    public static LenientRecognitionPreference NewLenientRecognitionPreference(string code, string title)
    {
        return new LenientRecognitionPreference(
            code,
            title,
            "",
            "",
            0.0,
            0,
            LenientRecognitionPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientRecognitionPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientRecognitionPreferenceStatus next)
    {
        switch (Status)
        {
            case LenientRecognitionPreferenceStatus.Open:
                return next == LenientRecognitionPreferenceStatus.Archived;
            case LenientRecognitionPreferenceStatus.InReview:
                switch (next)
                {
                    case LenientRecognitionPreferenceStatus.Resolved:
                        return true;
                }
                return false;
            case LenientRecognitionPreferenceStatus.Resolved:
                switch (next)
                {
                    case LenientRecognitionPreferenceStatus.Reopened:
                        return true;
                }
                return false;
            case LenientRecognitionPreferenceStatus.Reopened:
                return next == LenientRecognitionPreferenceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientRecognitionPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient recognition preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Fte + (double)Attempts, 2);
    }

    public LenientRecognitionPreference Copy()
    {
        return new LenientRecognitionPreference(Code, Title, Summary, EmployeeId, Fte, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
