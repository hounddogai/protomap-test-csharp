namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pulse survey manifest is in its life cycle.</summary>
public enum TieredPulseSurveyManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One tiered pulse survey manifest, with what the planners need to act on it.</summary>
public sealed class TieredPulseSurveyManifest(string code, string title, string jobCode, int createdAt, int updatedAt, TieredPulseSurveyManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public int CreatedAt { get; set; } = createdAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public TieredPulseSurveyManifestStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pulse survey manifest in status open.</summary>
    public static TieredPulseSurveyManifest NewTieredPulseSurveyManifest(string code, string title)
    {
        return new TieredPulseSurveyManifest(code, title, "", 0, 0, TieredPulseSurveyManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPulseSurveyManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPulseSurveyManifestStatus next)
    {
        switch (Status)
        {
            case TieredPulseSurveyManifestStatus.Open:
                return next == TieredPulseSurveyManifestStatus.Resolved;
            case TieredPulseSurveyManifestStatus.InReview:
                return next == TieredPulseSurveyManifestStatus.Archived;
            case TieredPulseSurveyManifestStatus.Resolved:
                switch (next)
                {
                    case TieredPulseSurveyManifestStatus.Reopened:
                        return true;
                }
                return false;
            case TieredPulseSurveyManifestStatus.Reopened:
                return next == TieredPulseSurveyManifestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TieredPulseSurveyManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pulse survey manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"tiered pulse survey manifest"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)CreatedAt + (double)UpdatedAt, 2);
    }

    public TieredPulseSurveyManifest Copy()
    {
        return new TieredPulseSurveyManifest(Code, Title, JobCode, CreatedAt, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
