namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg adjustment is in its life cycle.</summary>
public enum ReorgAdjustmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a reorg adjustment and its current status.</summary>
public sealed class ReorgAdjustment(string code, string label, bool archived, string location, string badge, double amount, ReorgAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string Location { get; set; } = location;
    public string Badge { get; set; } = badge;
    public double Amount { get; set; } = amount;
    public ReorgAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a reorg adjustment in status open.</summary>
    public static ReorgAdjustment NewReorgAdjustment(string code, string label)
    {
        return new ReorgAdjustment(code, label, false, "", "", 0.0, ReorgAdjustmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgAdjustmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgAdjustmentStatus next)
    {
        switch (Status)
        {
            case ReorgAdjustmentStatus.Open:
                switch (next)
                {
                    case ReorgAdjustmentStatus.InReview:
                        return true;
                }
                return false;
            case ReorgAdjustmentStatus.InReview:
                switch (next)
                {
                    case ReorgAdjustmentStatus.Reopened:
                    case ReorgAdjustmentStatus.Archived:
                        return true;
                }
                return false;
            case ReorgAdjustmentStatus.Resolved:
                return next == ReorgAdjustmentStatus.Reopened;
            case ReorgAdjustmentStatus.Reopened:
                switch (next)
                {
                    case ReorgAdjustmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ReorgAdjustment Copy()
    {
        return new ReorgAdjustment(Code, Label, Archived, Location, Badge, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
