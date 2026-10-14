namespace Acme.Workforce.Absence.Balance.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance batch is in its life cycle.</summary>
public enum BalanceBatchStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a balance batch and its current status.</summary>
public sealed class BalanceBatch(string code, string name, string team, int windowDays, double amount, double minimum, string description, string unit, string region, int revision, BalanceBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public int WindowDays { get; set; } = windowDays;
    public double Amount { get; set; } = amount;
    public double Minimum { get; set; } = minimum;
    public string Description { get; set; } = description;
    public string Unit { get; set; } = unit;
    public string Region { get; set; } = region;
    public int Revision { get; set; } = revision;
    public BalanceBatchStatus Status { get; set; } = status;

    /// <summary>Starts a balance batch in status draft.</summary>
    public static BalanceBatch NewBalanceBatch(string code, string name)
    {
        return new BalanceBatch(code, name, "", 0, 0.0, 0.0, "", "", "", 0, BalanceBatchStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceBatchStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceBatchStatus next)
    {
        switch (Status)
        {
            case BalanceBatchStatus.Draft:
                return next == BalanceBatchStatus.Active;
            case BalanceBatchStatus.Pending:
                switch (next)
                {
                    case BalanceBatchStatus.Active:
                        return true;
                }
                return false;
            case BalanceBatchStatus.Active:
                return next == BalanceBatchStatus.Suspended || next == BalanceBatchStatus.Closed;
            case BalanceBatchStatus.Suspended:
                return next == BalanceBatchStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BalanceBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Revision * Amount, 2);
    }

    public BalanceBatch Copy()
    {
        return new BalanceBatch(
            Code,
            Name,
            Team,
            WindowDays,
            Amount,
            Minimum,
            Description,
            Unit,
            Region,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
