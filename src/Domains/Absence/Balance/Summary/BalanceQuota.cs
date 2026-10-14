namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance quota is in its life cycle.</summary>
public enum BalanceQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A balance quota as the balance team keeps it.</summary>
public sealed class BalanceQuota(string code, string label, string phone, string timezone, double rate, int windowDays, bool archived, string unit, string currency, string checksum, int durationMinutes, BalanceQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public string Timezone { get; set; } = timezone;
    public double Rate { get; set; } = rate;
    public int WindowDays { get; set; } = windowDays;
    public bool Archived { get; set; } = archived;
    public string Unit { get; set; } = unit;
    public string Currency { get; set; } = currency;
    public string Checksum { get; set; } = checksum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public BalanceQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a balance quota in status created.</summary>
    public static BalanceQuota NewBalanceQuota(string code, string label)
    {
        return new BalanceQuota(code, label, "", "", 0.0, 0, false, "", "", "", 0, BalanceQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceQuotaStatus next)
    {
        switch (Status)
        {
            case BalanceQuotaStatus.Created:
                switch (next)
                {
                    case BalanceQuotaStatus.Validated:
                        return true;
                }
                return false;
            case BalanceQuotaStatus.Validated:
                switch (next)
                {
                    case BalanceQuotaStatus.Published:
                        return true;
                }
                return false;
            case BalanceQuotaStatus.Published:
                return next == BalanceQuotaStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BalanceQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)WindowDays * Rate, 2);
    }

    public BalanceQuota Copy()
    {
        return new BalanceQuota(
            Code,
            Label,
            Phone,
            Timezone,
            Rate,
            WindowDays,
            Archived,
            Unit,
            Currency,
            Checksum,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
