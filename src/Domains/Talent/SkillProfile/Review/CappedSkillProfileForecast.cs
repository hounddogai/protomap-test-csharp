namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped skill profile forecast is in its life cycle.</summary>
public enum CappedSkillProfileForecastStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a capped skill profile forecast and its current status.</summary>
public sealed class CappedSkillProfileForecast(string code, string label, string salaryBand, double minimum, string team, int endsAt, string segment, CappedSkillProfileForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public double Minimum { get; set; } = minimum;
    public string Team { get; set; } = team;
    public int EndsAt { get; set; } = endsAt;
    public string Segment { get; set; } = segment;
    public CappedSkillProfileForecastStatus Status { get; set; } = status;

    /// <summary>Starts a capped skill profile forecast in status requested.</summary>
    public static CappedSkillProfileForecast NewCappedSkillProfileForecast(string code, string label)
    {
        return new CappedSkillProfileForecast(
            code,
            label,
            "",
            0.0,
            "",
            0,
            "",
            CappedSkillProfileForecastStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedSkillProfileForecastStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedSkillProfileForecastStatus next)
    {
        switch (Status)
        {
            case CappedSkillProfileForecastStatus.Requested:
                return next == CappedSkillProfileForecastStatus.Revoked;
            case CappedSkillProfileForecastStatus.Granted:
                return next == CappedSkillProfileForecastStatus.Expired;
            case CappedSkillProfileForecastStatus.Revoked:
                return next == CappedSkillProfileForecastStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CappedSkillProfileForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped skill profile forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"capped skill profile forecast"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Minimum * (double)EndsAt, 2);
    }

    public CappedSkillProfileForecast Copy()
    {
        return new CappedSkillProfileForecast(Code, Label, SalaryBand, Minimum, Team, EndsAt, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
