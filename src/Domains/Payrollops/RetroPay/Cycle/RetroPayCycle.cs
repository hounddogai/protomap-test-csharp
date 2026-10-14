namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay cycle is in its life cycle.</summary>
public enum RetroPayCycleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a retro pay cycle and its current status.</summary>
public sealed class RetroPayCycle(string code, string name, string currency, int expiresAt, string notes, int limit, double discount, RetroPayCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Notes { get; set; } = notes;
    public int Limit { get; set; } = limit;
    public double Discount { get; set; } = discount;
    public RetroPayCycleStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay cycle in status draft.</summary>
    public static RetroPayCycle NewRetroPayCycle(string code, string name)
    {
        return new RetroPayCycle(code, name, "", 0, "", 0, 0.0, RetroPayCycleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayCycleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayCycleStatus next)
    {
        switch (Status)
        {
            case RetroPayCycleStatus.Draft:
                return next == RetroPayCycleStatus.Active || next == RetroPayCycleStatus.Suspended;
            case RetroPayCycleStatus.Pending:
                switch (next)
                {
                    case RetroPayCycleStatus.Active:
                    case RetroPayCycleStatus.Closed:
                        return true;
                }
                return false;
            case RetroPayCycleStatus.Active:
                return next == RetroPayCycleStatus.Suspended || next == RetroPayCycleStatus.Closed;
            case RetroPayCycleStatus.Suspended:
                switch (next)
                {
                    case RetroPayCycleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Limit + (double)ExpiresAt, 2);
    }

    public RetroPayCycle Copy()
    {
        return new RetroPayCycle(Code, Name, Currency, ExpiresAt, Notes, Limit, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
