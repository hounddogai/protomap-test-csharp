namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual nomination score is in its life cycle.</summary>
public enum ManualNominationScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a manual nomination score and its current status.</summary>
public sealed class ManualNominationScore(string code, string title, string managerId, double maximum, double score, string summary, double ceiling, double rating, string country, ManualNominationScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public double Maximum { get; set; } = maximum;
    public double Score { get; set; } = score;
    public string Summary { get; set; } = summary;
    public double Ceiling { get; set; } = ceiling;
    public double Rating { get; set; } = rating;
    public string Country { get; set; } = country;
    public ManualNominationScoreStatus Status { get; set; } = status;

    /// <summary>Starts a manual nomination score in status requested.</summary>
    public static ManualNominationScore NewManualNominationScore(string code, string title)
    {
        return new ManualNominationScore(
            code,
            title,
            "",
            0.0,
            0.0,
            "",
            0.0,
            0.0,
            "",
            ManualNominationScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualNominationScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualNominationScoreStatus next)
    {
        switch (Status)
        {
            case ManualNominationScoreStatus.Requested:
                return next == ManualNominationScoreStatus.Revoked || next == ManualNominationScoreStatus.Expired;
            case ManualNominationScoreStatus.Granted:
                switch (next)
                {
                    case ManualNominationScoreStatus.Revoked:
                    case ManualNominationScoreStatus.Expired:
                        return true;
                }
                return false;
            case ManualNominationScoreStatus.Revoked:
                switch (next)
                {
                    case ManualNominationScoreStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualNominationScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual nomination score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetVolume()
    {
        return Math.Round(Maximum - Score, 2);
    }

    public ManualNominationScore Copy()
    {
        return new ManualNominationScore(
            Code,
            Title,
            ManagerId,
            Maximum,
            Score,
            Summary,
            Ceiling,
            Rating,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
