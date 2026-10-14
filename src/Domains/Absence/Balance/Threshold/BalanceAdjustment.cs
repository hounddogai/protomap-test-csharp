namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance adjustment is in its life cycle.</summary>
public enum BalanceAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a balance adjustment and its current status.</summary>
public sealed class BalanceAdjustment(string code, string title, string description, string costCenter, string externalRef, int windowDays, BalanceAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string CostCenter { get; set; } = costCenter;
    public string ExternalRef { get; set; } = externalRef;
    public int WindowDays { get; set; } = windowDays;
    public BalanceAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a balance adjustment in status idle.</summary>
    public static BalanceAdjustment NewBalanceAdjustment(string code, string title)
    {
        return new BalanceAdjustment(code, title, "", "", "", 0, BalanceAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceAdjustmentStatus next)
    {
        switch (Status)
        {
            case BalanceAdjustmentStatus.Idle:
                switch (next)
                {
                    case BalanceAdjustmentStatus.Warming:
                        return true;
                }
                return false;
            case BalanceAdjustmentStatus.Warming:
                switch (next)
                {
                    case BalanceAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case BalanceAdjustmentStatus.Ready:
                switch (next)
                {
                    case BalanceAdjustmentStatus.Draining:
                    case BalanceAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case BalanceAdjustmentStatus.Draining:
                switch (next)
                {
                    case BalanceAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public BalanceAdjustment Copy()
    {
        return new BalanceAdjustment(Code, Title, Description, CostCenter, ExternalRef, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
