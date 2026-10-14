namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual pulse survey digest is in its life cycle.</summary>
public enum ManualPulseSurveyDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A manual pulse survey digest. Its code is unique within a tenant.</summary>
public sealed class ManualPulseSurveyDigest(string code, string name, bool visible, bool approved, string title, string summary, int graceDays, double budget, int limit, ManualPulseSurveyDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public bool Approved { get; set; } = approved;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public int GraceDays { get; set; } = graceDays;
    public double Budget { get; set; } = budget;
    public int Limit { get; set; } = limit;
    public ManualPulseSurveyDigestStatus Status { get; set; } = status;

    /// <summary>Starts a manual pulse survey digest in status idle.</summary>
    public static ManualPulseSurveyDigest NewManualPulseSurveyDigest(string code, string name)
    {
        return new ManualPulseSurveyDigest(
            code,
            name,
            false,
            false,
            "",
            "",
            0,
            0.0,
            0,
            ManualPulseSurveyDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPulseSurveyDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPulseSurveyDigestStatus next)
    {
        switch (Status)
        {
            case ManualPulseSurveyDigestStatus.Idle:
                switch (next)
                {
                    case ManualPulseSurveyDigestStatus.Draining:
                    case ManualPulseSurveyDigestStatus.Stopped:
                        return true;
                }
                return false;
            case ManualPulseSurveyDigestStatus.Warming:
                switch (next)
                {
                    case ManualPulseSurveyDigestStatus.Stopped:
                        return true;
                }
                return false;
            case ManualPulseSurveyDigestStatus.Ready:
                switch (next)
                {
                    case ManualPulseSurveyDigestStatus.Stopped:
                        return true;
                }
                return false;
            case ManualPulseSurveyDigestStatus.Draining:
                return next == ManualPulseSurveyDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualPulseSurveyDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual pulse survey digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual pulse survey digest"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Limit * Budget, 2);
    }

    public ManualPulseSurveyDigest Copy()
    {
        return new ManualPulseSurveyDigest(
            Code,
            Name,
            Visible,
            Approved,
            Title,
            Summary,
            GraceDays,
            Budget,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
