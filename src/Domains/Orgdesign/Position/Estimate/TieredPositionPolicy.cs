namespace Acme.Workforce.Orgdesign.Position.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered position policy is in its life cycle.</summary>
public enum TieredPositionPolicyStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered position policy as the position team keeps it.</summary>
public sealed class TieredPositionPolicy(string code, string title, string description, int capacity, int batchSize, TieredPositionPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public int Capacity { get; set; } = capacity;
    public int BatchSize { get; set; } = batchSize;
    public TieredPositionPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a tiered position policy in status proposed.</summary>
    public static TieredPositionPolicy NewTieredPositionPolicy(string code, string title)
    {
        return new TieredPositionPolicy(code, title, "", 0, 0, TieredPositionPolicyStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPositionPolicyStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPositionPolicyStatus next)
    {
        switch (Status)
        {
            case TieredPositionPolicyStatus.Proposed:
                return next == TieredPositionPolicyStatus.Approved || next == TieredPositionPolicyStatus.Withdrawn;
            case TieredPositionPolicyStatus.Approved:
                return next == TieredPositionPolicyStatus.Rejected || next == TieredPositionPolicyStatus.Withdrawn;
            case TieredPositionPolicyStatus.Rejected:
                return next == TieredPositionPolicyStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TieredPositionPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered position policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Capacity - (double)BatchSize, 2);
    }

    public TieredPositionPolicy Copy()
    {
        return new TieredPositionPolicy(Code, Title, Description, Capacity, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
