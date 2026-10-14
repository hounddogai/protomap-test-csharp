namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance policy is in its life cycle.</summary>
public enum BalancePolicyStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A balance policy as the balance team keeps it.</summary>
public sealed class BalancePolicy(string code, string name, int leaveDays, int startsAt, string country, double rating, int capacity, string costCenter, double amount, string source, BalancePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public int StartsAt { get; set; } = startsAt;
    public string Country { get; set; } = country;
    public double Rating { get; set; } = rating;
    public int Capacity { get; set; } = capacity;
    public string CostCenter { get; set; } = costCenter;
    public double Amount { get; set; } = amount;
    public string Source { get; set; } = source;
    public BalancePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a balance policy in status draft.</summary>
    public static BalancePolicy NewBalancePolicy(string code, string name)
    {
        return new BalancePolicy(code, name, 0, 0, "", 0.0, 0, "", 0.0, "", BalancePolicyStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalancePolicyStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalancePolicyStatus next)
    {
        switch (Status)
        {
            case BalancePolicyStatus.Draft:
                switch (next)
                {
                    case BalancePolicyStatus.Active:
                    case BalancePolicyStatus.Suspended:
                        return true;
                }
                return false;
            case BalancePolicyStatus.Pending:
                return next == BalancePolicyStatus.Active || next == BalancePolicyStatus.Closed;
            case BalancePolicyStatus.Active:
                switch (next)
                {
                    case BalancePolicyStatus.Closed:
                        return true;
                }
                return false;
            case BalancePolicyStatus.Suspended:
                return next == BalancePolicyStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BalancePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)StartsAt + Rating, 2);
    }

    public BalancePolicy Copy()
    {
        return new BalancePolicy(
            Code,
            Name,
            LeaveDays,
            StartsAt,
            Country,
            Rating,
            Capacity,
            CostCenter,
            Amount,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
