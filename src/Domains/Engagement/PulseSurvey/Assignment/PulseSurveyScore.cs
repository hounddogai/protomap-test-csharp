namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey score is in its life cycle.</summary>
public enum PulseSurveyScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A pulse survey score. Its code is unique within a tenant.</summary>
public sealed class PulseSurveyScore(string code, string name, int createdAt, bool archived, string unit, string timezone, int graceDays, string team, string region, string label, PulseSurveyScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public bool Archived { get; set; } = archived;
    public string Unit { get; set; } = unit;
    public string Timezone { get; set; } = timezone;
    public int GraceDays { get; set; } = graceDays;
    public string Team { get; set; } = team;
    public string Region { get; set; } = region;
    public string Label { get; set; } = label;
    public PulseSurveyScoreStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey score in status idle.</summary>
    public static PulseSurveyScore NewPulseSurveyScore(string code, string name)
    {
        return new PulseSurveyScore(code, name, 0, false, "", "", 0, "", "", "", PulseSurveyScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyScoreStatus next)
    {
        switch (Status)
        {
            case PulseSurveyScoreStatus.Idle:
                switch (next)
                {
                    case PulseSurveyScoreStatus.Ready:
                        return true;
                }
                return false;
            case PulseSurveyScoreStatus.Warming:
                return next == PulseSurveyScoreStatus.Stopped;
            case PulseSurveyScoreStatus.Ready:
                return next == PulseSurveyScoreStatus.Draining;
            case PulseSurveyScoreStatus.Draining:
                switch (next)
                {
                    case PulseSurveyScoreStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double NetVolume()
    {
        return Math.Round((double)CreatedAt - (double)GraceDays, 2);
    }

    public PulseSurveyScore Copy()
    {
        return new PulseSurveyScore(
            Code,
            Name,
            CreatedAt,
            Archived,
            Unit,
            Timezone,
            GraceDays,
            Team,
            Region,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
