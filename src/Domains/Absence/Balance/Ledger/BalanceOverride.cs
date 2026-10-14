namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance override is in its life cycle.</summary>
public enum BalanceOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a balance override and its current status.</summary>
public sealed class BalanceOverride(string code, string name, double minimum, int expiresAt, int sequence, string locale, string region, BalanceOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Sequence { get; set; } = sequence;
    public string Locale { get; set; } = locale;
    public string Region { get; set; } = region;
    public BalanceOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a balance override in status new.</summary>
    public static BalanceOverride NewBalanceOverride(string code, string name)
    {
        return new BalanceOverride(code, name, 0.0, 0, 0, "", "", BalanceOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceOverrideStatus next)
    {
        switch (Status)
        {
            case BalanceOverrideStatus.New:
                return next == BalanceOverrideStatus.Failed;
            case BalanceOverrideStatus.Queued:
                return next == BalanceOverrideStatus.Done || next == BalanceOverrideStatus.Failed;
            case BalanceOverrideStatus.Running:
                return next == BalanceOverrideStatus.Done || next == BalanceOverrideStatus.Failed;
            case BalanceOverrideStatus.Done:
                switch (next)
                {
                    case BalanceOverrideStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)ExpiresAt * (double)Sequence, 2);
    }

    public BalanceOverride Copy()
    {
        return new BalanceOverride(Code, Name, Minimum, ExpiresAt, Sequence, Locale, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
