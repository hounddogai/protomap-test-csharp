namespace Acme.Workforce.Contractors.Engagement.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered engagement adjustment is in its life cycle.</summary>
public enum TieredEngagementAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One tiered engagement adjustment, with what the planners need to act on it.</summary>
public sealed class TieredEngagementAdjustment(string code, string name, string label, int quantity, int endsAt, double amount, TieredEngagementAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public int EndsAt { get; set; } = endsAt;
    public double Amount { get; set; } = amount;
    public TieredEngagementAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a tiered engagement adjustment in status new.</summary>
    public static TieredEngagementAdjustment NewTieredEngagementAdjustment(string code, string name)
    {
        return new TieredEngagementAdjustment(code, name, "", 0, 0, 0.0, TieredEngagementAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredEngagementAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredEngagementAdjustmentStatus next)
    {
        switch (Status)
        {
            case TieredEngagementAdjustmentStatus.New:
                switch (next)
                {
                    case TieredEngagementAdjustmentStatus.Queued:
                    case TieredEngagementAdjustmentStatus.Done:
                        return true;
                }
                return false;
            case TieredEngagementAdjustmentStatus.Queued:
                switch (next)
                {
                    case TieredEngagementAdjustmentStatus.Done:
                        return true;
                }
                return false;
            case TieredEngagementAdjustmentStatus.Running:
                switch (next)
                {
                    case TieredEngagementAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case TieredEngagementAdjustmentStatus.Done:
                switch (next)
                {
                    case TieredEngagementAdjustmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredEngagementAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered engagement adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)EndsAt + (double)Quantity, 2);
    }

    public TieredEngagementAdjustment Copy()
    {
        return new TieredEngagementAdjustment(Code, Name, Label, Quantity, EndsAt, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
