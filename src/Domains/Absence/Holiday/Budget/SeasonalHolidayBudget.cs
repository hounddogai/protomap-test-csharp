namespace Acme.Workforce.Absence.Holiday.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal holiday budget is in its life cycle.</summary>
public enum SeasonalHolidayBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a seasonal holiday budget and its current status.</summary>
public sealed class SeasonalHolidayBudget(string code, string title, string source, string team, bool pinned, int endsAt, double weight, SeasonalHolidayBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Team { get; set; } = team;
    public bool Pinned { get; set; } = pinned;
    public int EndsAt { get; set; } = endsAt;
    public double Weight { get; set; } = weight;
    public SeasonalHolidayBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal holiday budget in status requested.</summary>
    public static SeasonalHolidayBudget NewSeasonalHolidayBudget(string code, string title)
    {
        return new SeasonalHolidayBudget(code, title, "", "", false, 0, 0.0, SeasonalHolidayBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalHolidayBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalHolidayBudgetStatus next)
    {
        switch (Status)
        {
            case SeasonalHolidayBudgetStatus.Requested:
                switch (next)
                {
                    case SeasonalHolidayBudgetStatus.Revoked:
                        return true;
                }
                return false;
            case SeasonalHolidayBudgetStatus.Granted:
                switch (next)
                {
                    case SeasonalHolidayBudgetStatus.Revoked:
                    case SeasonalHolidayBudgetStatus.Expired:
                        return true;
                }
                return false;
            case SeasonalHolidayBudgetStatus.Revoked:
                return next == SeasonalHolidayBudgetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SeasonalHolidayBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal holiday budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"seasonal holiday budget"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Weight * (double)EndsAt, 2);
    }

    public SeasonalHolidayBudget Copy()
    {
        return new SeasonalHolidayBudget(Code, Title, Source, Team, Pinned, EndsAt, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
