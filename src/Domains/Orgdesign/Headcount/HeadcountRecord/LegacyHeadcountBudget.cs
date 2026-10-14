namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy headcount budget is in its life cycle.</summary>
public enum LegacyHeadcountBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A legacy headcount budget. Its code is unique within a tenant.</summary>
public sealed class LegacyHeadcountBudget(string code, string title, int durationMinutes, double score, double rating, string phone, bool active, string name, LegacyHeadcountBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Score { get; set; } = score;
    public double Rating { get; set; } = rating;
    public string Phone { get; set; } = phone;
    public bool Active { get; set; } = active;
    public string Name { get; set; } = name;
    public LegacyHeadcountBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a legacy headcount budget in status requested.</summary>
    public static LegacyHeadcountBudget NewLegacyHeadcountBudget(string code, string title)
    {
        return new LegacyHeadcountBudget(
            code,
            title,
            0,
            0.0,
            0.0,
            "",
            false,
            "",
            LegacyHeadcountBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyHeadcountBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyHeadcountBudgetStatus next)
    {
        switch (Status)
        {
            case LegacyHeadcountBudgetStatus.Requested:
                switch (next)
                {
                    case LegacyHeadcountBudgetStatus.Granted:
                    case LegacyHeadcountBudgetStatus.Revoked:
                        return true;
                }
                return false;
            case LegacyHeadcountBudgetStatus.Granted:
                return next == LegacyHeadcountBudgetStatus.Expired;
            case LegacyHeadcountBudgetStatus.Revoked:
                switch (next)
                {
                    case LegacyHeadcountBudgetStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyHeadcountBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy headcount budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetVolume()
    {
        return Math.Round(Score - Rating, 2);
    }

    public LegacyHeadcountBudget Copy()
    {
        return new LegacyHeadcountBudget(Code, Title, DurationMinutes, Score, Rating, Phone, Active, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
