namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance claim is in its life cycle.</summary>
public enum BalanceClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A balance claim as the balance team keeps it.</summary>
public sealed class BalanceClaim(string code, string name, string checksum, double fte, string employeeId, double amount, int attempts, bool archived, BalanceClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public double Fte { get; set; } = fte;
    public string EmployeeId { get; set; } = employeeId;
    public double Amount { get; set; } = amount;
    public int Attempts { get; set; } = attempts;
    public bool Archived { get; set; } = archived;
    public BalanceClaimStatus Status { get; set; } = status;

    /// <summary>Starts a balance claim in status created.</summary>
    public static BalanceClaim NewBalanceClaim(string code, string name)
    {
        return new BalanceClaim(code, name, "", 0.0, "", 0.0, 0, false, BalanceClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceClaimStatus next)
    {
        switch (Status)
        {
            case BalanceClaimStatus.Created:
                switch (next)
                {
                    case BalanceClaimStatus.Validated:
                    case BalanceClaimStatus.Retired:
                        return true;
                }
                return false;
            case BalanceClaimStatus.Validated:
                switch (next)
                {
                    case BalanceClaimStatus.Published:
                    case BalanceClaimStatus.Retired:
                        return true;
                }
                return false;
            case BalanceClaimStatus.Published:
                return next == BalanceClaimStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BalanceClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Fte * (double)Attempts, 2);
    }

    public BalanceClaim Copy()
    {
        return new BalanceClaim(Code, Name, Checksum, Fte, EmployeeId, Amount, Attempts, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
