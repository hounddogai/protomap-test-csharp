namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal holiday rollup is in its life cycle.</summary>
public enum SeasonalHolidayRollupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One seasonal holiday rollup, with what the planners need to act on it.</summary>
public sealed class SeasonalHolidayRollup(string code, string label, string team, string contactEmail, int quantity, int endsAt, double weight, double cost, string notes, SeasonalHolidayRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string ContactEmail { get; set; } = contactEmail;
    public int Quantity { get; set; } = quantity;
    public int EndsAt { get; set; } = endsAt;
    public double Weight { get; set; } = weight;
    public double Cost { get; set; } = cost;
    public string Notes { get; set; } = notes;
    public SeasonalHolidayRollupStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal holiday rollup in status open.</summary>
    public static SeasonalHolidayRollup NewSeasonalHolidayRollup(string code, string label)
    {
        return new SeasonalHolidayRollup(code, label, "", "", 0, 0, 0.0, 0.0, "", SeasonalHolidayRollupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalHolidayRollupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalHolidayRollupStatus next)
    {
        switch (Status)
        {
            case SeasonalHolidayRollupStatus.Open:
                switch (next)
                {
                    case SeasonalHolidayRollupStatus.Archived:
                        return true;
                }
                return false;
            case SeasonalHolidayRollupStatus.InReview:
                switch (next)
                {
                    case SeasonalHolidayRollupStatus.Archived:
                        return true;
                }
                return false;
            case SeasonalHolidayRollupStatus.Resolved:
                return next == SeasonalHolidayRollupStatus.Reopened || next == SeasonalHolidayRollupStatus.Archived;
            case SeasonalHolidayRollupStatus.Reopened:
                return next == SeasonalHolidayRollupStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SeasonalHolidayRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal holiday rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Cost * Weight, 2);
    }

    public SeasonalHolidayRollup Copy()
    {
        return new SeasonalHolidayRollup(
            Code,
            Label,
            Team,
            ContactEmail,
            Quantity,
            EndsAt,
            Weight,
            Cost,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
