namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance baseline is in its life cycle.</summary>
public enum BalanceBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A balance baseline as the balance team keeps it.</summary>
public sealed class BalanceBaseline(string code, string title, string currency, bool approved, string externalRef, double margin, string region, int leaveDays, string timezone, string grade, BalanceBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public bool Approved { get; set; } = approved;
    public string ExternalRef { get; set; } = externalRef;
    public double Margin { get; set; } = margin;
    public string Region { get; set; } = region;
    public int LeaveDays { get; set; } = leaveDays;
    public string Timezone { get; set; } = timezone;
    public string Grade { get; set; } = grade;
    public BalanceBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a balance baseline in status requested.</summary>
    public static BalanceBaseline NewBalanceBaseline(string code, string title)
    {
        return new BalanceBaseline(code, title, "", false, "", 0.0, "", 0, "", "", BalanceBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceBaselineStatus next)
    {
        switch (Status)
        {
            case BalanceBaselineStatus.Requested:
                switch (next)
                {
                    case BalanceBaselineStatus.Expired:
                        return true;
                }
                return false;
            case BalanceBaselineStatus.Granted:
                switch (next)
                {
                    case BalanceBaselineStatus.Revoked:
                        return true;
                }
                return false;
            case BalanceBaselineStatus.Revoked:
                return next == BalanceBaselineStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BalanceBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"balance baseline"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)LeaveDays - Margin, 2);
    }

    public BalanceBaseline Copy()
    {
        return new BalanceBaseline(
            Code,
            Title,
            Currency,
            Approved,
            ExternalRef,
            Margin,
            Region,
            LeaveDays,
            Timezone,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
