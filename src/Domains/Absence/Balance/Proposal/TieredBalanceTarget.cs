namespace Acme.Workforce.Absence.Balance.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered balance target is in its life cycle.</summary>
public enum TieredBalanceTargetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One tiered balance target, with what the planners need to act on it.</summary>
public sealed class TieredBalanceTarget(string code, string name, int windowDays, int tenureMonths, string currency, TieredBalanceTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Currency { get; set; } = currency;
    public TieredBalanceTargetStatus Status { get; set; } = status;

    /// <summary>Starts a tiered balance target in status draft.</summary>
    public static TieredBalanceTarget NewTieredBalanceTarget(string code, string name)
    {
        return new TieredBalanceTarget(code, name, 0, 0, "", TieredBalanceTargetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredBalanceTargetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredBalanceTargetStatus next)
    {
        switch (Status)
        {
            case TieredBalanceTargetStatus.Draft:
                switch (next)
                {
                    case TieredBalanceTargetStatus.Pending:
                    case TieredBalanceTargetStatus.Suspended:
                        return true;
                }
                return false;
            case TieredBalanceTargetStatus.Pending:
                return next == TieredBalanceTargetStatus.Active || next == TieredBalanceTargetStatus.Closed;
            case TieredBalanceTargetStatus.Active:
                return next == TieredBalanceTargetStatus.Suspended;
            case TieredBalanceTargetStatus.Suspended:
                switch (next)
                {
                    case TieredBalanceTargetStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredBalanceTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered balance target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)WindowDays * (double)TenureMonths, 2);
    }

    public TieredBalanceTarget Copy()
    {
        return new TieredBalanceTarget(Code, Name, WindowDays, TenureMonths, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
