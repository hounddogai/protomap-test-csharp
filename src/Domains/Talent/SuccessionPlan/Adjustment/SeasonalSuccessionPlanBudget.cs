namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal succession plan budget is in its life cycle.</summary>
public enum SeasonalSuccessionPlanBudgetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A seasonal succession plan budget. Its code is unique within a tenant.</summary>
public sealed class SeasonalSuccessionPlanBudget(string code, string label, double rating, double cost, int updatedAt, double ratio, int startsAt, string source, string currency, SeasonalSuccessionPlanBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public double Cost { get; set; } = cost;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ratio { get; set; } = ratio;
    public int StartsAt { get; set; } = startsAt;
    public string Source { get; set; } = source;
    public string Currency { get; set; } = currency;
    public SeasonalSuccessionPlanBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal succession plan budget in status requested.</summary>
    public static SeasonalSuccessionPlanBudget NewSeasonalSuccessionPlanBudget(string code, string label)
    {
        return new SeasonalSuccessionPlanBudget(
            code,
            label,
            0.0,
            0.0,
            0,
            0.0,
            0,
            "",
            "",
            SeasonalSuccessionPlanBudgetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalSuccessionPlanBudgetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalSuccessionPlanBudgetStatus next)
    {
        switch (Status)
        {
            case SeasonalSuccessionPlanBudgetStatus.Requested:
                switch (next)
                {
                    case SeasonalSuccessionPlanBudgetStatus.Granted:
                        return true;
                }
                return false;
            case SeasonalSuccessionPlanBudgetStatus.Granted:
                switch (next)
                {
                    case SeasonalSuccessionPlanBudgetStatus.Revoked:
                    case SeasonalSuccessionPlanBudgetStatus.Expired:
                        return true;
                }
                return false;
            case SeasonalSuccessionPlanBudgetStatus.Revoked:
                return next == SeasonalSuccessionPlanBudgetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SeasonalSuccessionPlanBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal succession plan budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)StartsAt - Ratio, 2);
    }

    public SeasonalSuccessionPlanBudget Copy()
    {
        return new SeasonalSuccessionPlanBudget(
            Code,
            Label,
            Rating,
            Cost,
            UpdatedAt,
            Ratio,
            StartsAt,
            Source,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
