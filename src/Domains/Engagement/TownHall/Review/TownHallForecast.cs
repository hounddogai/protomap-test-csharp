namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall forecast is in its life cycle.</summary>
public enum TownHallForecastStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A town hall forecast. Its code is unique within a tenant.</summary>
public sealed class TownHallForecast(string code, string title, double budget, string externalRef, string checksum, double fte, double score, TownHallForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public string ExternalRef { get; set; } = externalRef;
    public string Checksum { get; set; } = checksum;
    public double Fte { get; set; } = fte;
    public double Score { get; set; } = score;
    public TownHallForecastStatus Status { get; set; } = status;

    /// <summary>Starts a town hall forecast in status requested.</summary>
    public static TownHallForecast NewTownHallForecast(string code, string title)
    {
        return new TownHallForecast(code, title, 0.0, "", "", 0.0, 0.0, TownHallForecastStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallForecastStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallForecastStatus next)
    {
        switch (Status)
        {
            case TownHallForecastStatus.Requested:
                switch (next)
                {
                    case TownHallForecastStatus.Revoked:
                    case TownHallForecastStatus.Expired:
                        return true;
                }
                return false;
            case TownHallForecastStatus.Granted:
                return next == TownHallForecastStatus.Expired;
            case TownHallForecastStatus.Revoked:
                return next == TownHallForecastStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TownHallForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Score * Budget, 2);
    }

    public TownHallForecast Copy()
    {
        return new TownHallForecast(Code, Title, Budget, ExternalRef, Checksum, Fte, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
