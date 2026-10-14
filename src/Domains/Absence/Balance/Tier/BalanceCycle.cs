namespace Acme.Workforce.Absence.Balance.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance cycle is in its life cycle.</summary>
public enum BalanceCycleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A balance cycle as the balance team keeps it.</summary>
public sealed class BalanceCycle(string code, string title, int attempts, int retries, double threshold, string ownerId, BalanceCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public string OwnerId { get; set; } = ownerId;
    public BalanceCycleStatus Status { get; set; } = status;

    /// <summary>Starts a balance cycle in status draft.</summary>
    public static BalanceCycle NewBalanceCycle(string code, string title)
    {
        return new BalanceCycle(code, title, 0, 0, 0.0, "", BalanceCycleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceCycleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceCycleStatus next)
    {
        switch (Status)
        {
            case BalanceCycleStatus.Draft:
                return next == BalanceCycleStatus.Pending || next == BalanceCycleStatus.Closed;
            case BalanceCycleStatus.Pending:
                switch (next)
                {
                    case BalanceCycleStatus.Active:
                    case BalanceCycleStatus.Suspended:
                        return true;
                }
                return false;
            case BalanceCycleStatus.Active:
                switch (next)
                {
                    case BalanceCycleStatus.Suspended:
                    case BalanceCycleStatus.Closed:
                        return true;
                }
                return false;
            case BalanceCycleStatus.Suspended:
                return next == BalanceCycleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BalanceCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Retries - Threshold, 2);
    }

    public BalanceCycle Copy()
    {
        return new BalanceCycle(Code, Title, Attempts, Retries, Threshold, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
