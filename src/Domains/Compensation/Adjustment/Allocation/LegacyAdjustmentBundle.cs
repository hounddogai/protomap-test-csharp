namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy adjustment bundle is in its life cycle.</summary>
public enum LegacyAdjustmentBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a legacy adjustment bundle and its current status.</summary>
public sealed class LegacyAdjustmentBundle(string code, string name, int tenureMonths, double amount, string summary, string phone, int durationMinutes, string contactEmail, LegacyAdjustmentBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Amount { get; set; } = amount;
    public string Summary { get; set; } = summary;
    public string Phone { get; set; } = phone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ContactEmail { get; set; } = contactEmail;
    public LegacyAdjustmentBundleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy adjustment bundle in status draft.</summary>
    public static LegacyAdjustmentBundle NewLegacyAdjustmentBundle(string code, string name)
    {
        return new LegacyAdjustmentBundle(code, name, 0, 0.0, "", "", 0, "", LegacyAdjustmentBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAdjustmentBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAdjustmentBundleStatus next)
    {
        switch (Status)
        {
            case LegacyAdjustmentBundleStatus.Draft:
                return next == LegacyAdjustmentBundleStatus.Active;
            case LegacyAdjustmentBundleStatus.Pending:
                switch (next)
                {
                    case LegacyAdjustmentBundleStatus.Suspended:
                    case LegacyAdjustmentBundleStatus.Closed:
                        return true;
                }
                return false;
            case LegacyAdjustmentBundleStatus.Active:
                switch (next)
                {
                    case LegacyAdjustmentBundleStatus.Suspended:
                    case LegacyAdjustmentBundleStatus.Closed:
                        return true;
                }
                return false;
            case LegacyAdjustmentBundleStatus.Suspended:
                switch (next)
                {
                    case LegacyAdjustmentBundleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyAdjustmentBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy adjustment bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveValue()
    {
        return Math.Round(Amount + (double)TenureMonths, 2);
    }

    public LegacyAdjustmentBundle Copy()
    {
        return new LegacyAdjustmentBundle(
            Code,
            Name,
            TenureMonths,
            Amount,
            Summary,
            Phone,
            DurationMinutes,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
