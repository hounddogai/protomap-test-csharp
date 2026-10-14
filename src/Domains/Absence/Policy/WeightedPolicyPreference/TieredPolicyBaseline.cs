namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered policy baseline is in its life cycle.</summary>
public enum TieredPolicyBaselineStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A tiered policy baseline. Its code is unique within a tenant.</summary>
public sealed class TieredPolicyBaseline(string code, string title, string salaryBand, string location, string checksum, string badge, string currency, string team, int capacity, TieredPolicyBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string Location { get; set; } = location;
    public string Checksum { get; set; } = checksum;
    public string Badge { get; set; } = badge;
    public string Currency { get; set; } = currency;
    public string Team { get; set; } = team;
    public int Capacity { get; set; } = capacity;
    public TieredPolicyBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a tiered policy baseline in status requested.</summary>
    public static TieredPolicyBaseline NewTieredPolicyBaseline(string code, string title)
    {
        return new TieredPolicyBaseline(
            code,
            title,
            "",
            "",
            "",
            "",
            "",
            "",
            0,
            TieredPolicyBaselineStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPolicyBaselineStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPolicyBaselineStatus next)
    {
        switch (Status)
        {
            case TieredPolicyBaselineStatus.Requested:
                return next == TieredPolicyBaselineStatus.Granted || next == TieredPolicyBaselineStatus.Expired;
            case TieredPolicyBaselineStatus.Granted:
                return next == TieredPolicyBaselineStatus.Revoked || next == TieredPolicyBaselineStatus.Expired;
            case TieredPolicyBaselineStatus.Revoked:
                switch (next)
                {
                    case TieredPolicyBaselineStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredPolicyBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered policy baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public TieredPolicyBaseline Copy()
    {
        return new TieredPolicyBaseline(
            Code,
            Title,
            SalaryBand,
            Location,
            Checksum,
            Badge,
            Currency,
            Team,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
