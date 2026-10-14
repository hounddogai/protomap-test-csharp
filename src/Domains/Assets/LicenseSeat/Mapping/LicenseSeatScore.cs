namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat score is in its life cycle.</summary>
public enum LicenseSeatScoreStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a license seat score and its current status.</summary>
public sealed class LicenseSeatScore(string code, string title, string unit, int durationMinutes, int endsAt, string skill, string reviewer, string contactEmail, bool pinned, double cost, LicenseSeatScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int EndsAt { get; set; } = endsAt;
    public string Skill { get; set; } = skill;
    public string Reviewer { get; set; } = reviewer;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Pinned { get; set; } = pinned;
    public double Cost { get; set; } = cost;
    public LicenseSeatScoreStatus Status { get; set; } = status;

    /// <summary>Starts a license seat score in status new.</summary>
    public static LicenseSeatScore NewLicenseSeatScore(string code, string title)
    {
        return new LicenseSeatScore(code, title, "", 0, 0, "", "", "", false, 0.0, LicenseSeatScoreStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatScoreStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatScoreStatus next)
    {
        switch (Status)
        {
            case LicenseSeatScoreStatus.New:
                return next == LicenseSeatScoreStatus.Queued || next == LicenseSeatScoreStatus.Done;
            case LicenseSeatScoreStatus.Queued:
                return next == LicenseSeatScoreStatus.Running;
            case LicenseSeatScoreStatus.Running:
                return next == LicenseSeatScoreStatus.Done || next == LicenseSeatScoreStatus.Failed;
            case LicenseSeatScoreStatus.Done:
                switch (next)
                {
                    case LicenseSeatScoreStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)EndsAt * Cost, 2);
    }

    public LicenseSeatScore Copy()
    {
        return new LicenseSeatScore(
            Code,
            Title,
            Unit,
            DurationMinutes,
            EndsAt,
            Skill,
            Reviewer,
            ContactEmail,
            Pinned,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
