namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pool group is in its life cycle.</summary>
public enum TieredPoolGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a tiered pool group and its current status.</summary>
public sealed class TieredPoolGroup(string code, string name, int sequence, int retries, bool visible, double amount, int capacity, string department, double score, int revision, TieredPoolGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public int Retries { get; set; } = retries;
    public bool Visible { get; set; } = visible;
    public double Amount { get; set; } = amount;
    public int Capacity { get; set; } = capacity;
    public string Department { get; set; } = department;
    public double Score { get; set; } = score;
    public int Revision { get; set; } = revision;
    public TieredPoolGroupStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pool group in status created.</summary>
    public static TieredPoolGroup NewTieredPoolGroup(string code, string name)
    {
        return new TieredPoolGroup(code, name, 0, 0, false, 0.0, 0, "", 0.0, 0, TieredPoolGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPoolGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPoolGroupStatus next)
    {
        switch (Status)
        {
            case TieredPoolGroupStatus.Created:
                switch (next)
                {
                    case TieredPoolGroupStatus.Retired:
                        return true;
                }
                return false;
            case TieredPoolGroupStatus.Validated:
                switch (next)
                {
                    case TieredPoolGroupStatus.Retired:
                        return true;
                }
                return false;
            case TieredPoolGroupStatus.Published:
                switch (next)
                {
                    case TieredPoolGroupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredPoolGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pool group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Capacity * (double)Sequence, 2);
    }

    public TieredPoolGroup Copy()
    {
        return new TieredPoolGroup(
            Code,
            Name,
            Sequence,
            Retries,
            Visible,
            Amount,
            Capacity,
            Department,
            Score,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
