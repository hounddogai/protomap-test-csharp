namespace Acme.Workforce.Engagement.PulseSurvey.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey batch is in its life cycle.</summary>
public enum PulseSurveyBatchStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pulse survey batch as the pulse survey team keeps it.</summary>
public sealed class PulseSurveyBatch(string code, string title, string region, string jobCode, bool active, int createdAt, double ratio, PulseSurveyBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string JobCode { get; set; } = jobCode;
    public bool Active { get; set; } = active;
    public int CreatedAt { get; set; } = createdAt;
    public double Ratio { get; set; } = ratio;
    public PulseSurveyBatchStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey batch in status idle.</summary>
    public static PulseSurveyBatch NewPulseSurveyBatch(string code, string title)
    {
        return new PulseSurveyBatch(code, title, "", "", false, 0, 0.0, PulseSurveyBatchStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyBatchStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyBatchStatus next)
    {
        switch (Status)
        {
            case PulseSurveyBatchStatus.Idle:
                switch (next)
                {
                    case PulseSurveyBatchStatus.Draining:
                        return true;
                }
                return false;
            case PulseSurveyBatchStatus.Warming:
                switch (next)
                {
                    case PulseSurveyBatchStatus.Ready:
                    case PulseSurveyBatchStatus.Stopped:
                        return true;
                }
                return false;
            case PulseSurveyBatchStatus.Ready:
                return next == PulseSurveyBatchStatus.Draining;
            case PulseSurveyBatchStatus.Draining:
                return next == PulseSurveyBatchStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PulseSurveyBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)CreatedAt - Ratio, 2);
    }

    public PulseSurveyBatch Copy()
    {
        return new PulseSurveyBatch(Code, Title, Region, JobCode, Active, CreatedAt, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
