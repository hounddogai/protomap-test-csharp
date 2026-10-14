namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance threshold is in its life cycle.</summary>
public enum BalanceThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a balance threshold and its current status.</summary>
public sealed class BalanceThreshold(string code, string name, bool active, double maximum, double discount, bool visible, double rate, BalanceThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public double Maximum { get; set; } = maximum;
    public double Discount { get; set; } = discount;
    public bool Visible { get; set; } = visible;
    public double Rate { get; set; } = rate;
    public BalanceThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a balance threshold in status requested.</summary>
    public static BalanceThreshold NewBalanceThreshold(string code, string name)
    {
        return new BalanceThreshold(code, name, false, 0.0, 0.0, false, 0.0, BalanceThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceThresholdStatus next)
    {
        switch (Status)
        {
            case BalanceThresholdStatus.Requested:
                switch (next)
                {
                    case BalanceThresholdStatus.Expired:
                        return true;
                }
                return false;
            case BalanceThresholdStatus.Granted:
                return next == BalanceThresholdStatus.Revoked || next == BalanceThresholdStatus.Expired;
            case BalanceThresholdStatus.Revoked:
                switch (next)
                {
                    case BalanceThresholdStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"balance threshold"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Discount - Maximum, 2);
    }

    public BalanceThreshold Copy()
    {
        return new BalanceThreshold(Code, Name, Active, Maximum, Discount, Visible, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
