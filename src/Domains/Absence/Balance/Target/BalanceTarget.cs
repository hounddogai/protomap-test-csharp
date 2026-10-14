namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance target is in its life cycle.</summary>
public enum BalanceTargetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One balance target, with what the planners need to act on it.</summary>
public sealed class BalanceTarget(string code, string name, string timezone, string title, string jobCode, string unit, int quantity, BalanceTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public string Unit { get; set; } = unit;
    public int Quantity { get; set; } = quantity;
    public BalanceTargetStatus Status { get; set; } = status;

    /// <summary>Starts a balance target in status open.</summary>
    public static BalanceTarget NewBalanceTarget(string code, string name)
    {
        return new BalanceTarget(code, name, "", "", "", "", 0, BalanceTargetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceTargetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceTargetStatus next)
    {
        switch (Status)
        {
            case BalanceTargetStatus.Open:
                switch (next)
                {
                    case BalanceTargetStatus.InReview:
                        return true;
                }
                return false;
            case BalanceTargetStatus.InReview:
                switch (next)
                {
                    case BalanceTargetStatus.Resolved:
                        return true;
                }
                return false;
            case BalanceTargetStatus.Resolved:
                switch (next)
                {
                    case BalanceTargetStatus.Archived:
                        return true;
                }
                return false;
            case BalanceTargetStatus.Reopened:
                switch (next)
                {
                    case BalanceTargetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public BalanceTarget Copy()
    {
        return new BalanceTarget(Code, Name, Timezone, Title, JobCode, Unit, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
