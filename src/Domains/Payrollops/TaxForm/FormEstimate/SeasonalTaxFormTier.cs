namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal tax form tier is in its life cycle.</summary>
public enum SeasonalTaxFormTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a seasonal tax form tier and its current status.</summary>
public sealed class SeasonalTaxFormTier(string code, string label, int createdAt, string title, double maximum, string currency, string costCenter, int updatedAt, double minimum, string badge, string team, SeasonalTaxFormTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string Currency { get; set; } = currency;
    public string CostCenter { get; set; } = costCenter;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Minimum { get; set; } = minimum;
    public string Badge { get; set; } = badge;
    public string Team { get; set; } = team;
    public SeasonalTaxFormTierStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal tax form tier in status created.</summary>
    public static SeasonalTaxFormTier NewSeasonalTaxFormTier(string code, string label)
    {
        return new SeasonalTaxFormTier(
            code,
            label,
            0,
            "",
            0.0,
            "",
            "",
            0,
            0.0,
            "",
            "",
            SeasonalTaxFormTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalTaxFormTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalTaxFormTierStatus next)
    {
        switch (Status)
        {
            case SeasonalTaxFormTierStatus.Created:
                return next == SeasonalTaxFormTierStatus.Published || next == SeasonalTaxFormTierStatus.Retired;
            case SeasonalTaxFormTierStatus.Validated:
                return next == SeasonalTaxFormTierStatus.Retired;
            case SeasonalTaxFormTierStatus.Published:
                switch (next)
                {
                    case SeasonalTaxFormTierStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalTaxFormTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal tax form tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Minimum + (double)CreatedAt, 2);
    }

    public SeasonalTaxFormTier Copy()
    {
        return new SeasonalTaxFormTier(
            Code,
            Label,
            CreatedAt,
            Title,
            Maximum,
            Currency,
            CostCenter,
            UpdatedAt,
            Minimum,
            Badge,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
