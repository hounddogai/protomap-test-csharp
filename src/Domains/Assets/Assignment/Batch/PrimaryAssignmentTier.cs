namespace Acme.Workforce.Assets.Assignment.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary assignment tier is in its life cycle.</summary>
public enum PrimaryAssignmentTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary assignment tier as the assignment team keeps it.</summary>
public sealed class PrimaryAssignmentTier(string code, string label, double amount, bool archived, int sequence, double score, string country, PrimaryAssignmentTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public bool Archived { get; set; } = archived;
    public int Sequence { get; set; } = sequence;
    public double Score { get; set; } = score;
    public string Country { get; set; } = country;
    public PrimaryAssignmentTierStatus Status { get; set; } = status;

    /// <summary>Starts a primary assignment tier in status open.</summary>
    public static PrimaryAssignmentTier NewPrimaryAssignmentTier(string code, string label)
    {
        return new PrimaryAssignmentTier(code, label, 0.0, false, 0, 0.0, "", PrimaryAssignmentTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAssignmentTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAssignmentTierStatus next)
    {
        switch (Status)
        {
            case PrimaryAssignmentTierStatus.Open:
                return next == PrimaryAssignmentTierStatus.Archived;
            case PrimaryAssignmentTierStatus.InReview:
                return next == PrimaryAssignmentTierStatus.Resolved || next == PrimaryAssignmentTierStatus.Archived;
            case PrimaryAssignmentTierStatus.Resolved:
                switch (next)
                {
                    case PrimaryAssignmentTierStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryAssignmentTierStatus.Reopened:
                return next == PrimaryAssignmentTierStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryAssignmentTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary assignment tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedValue()
    {
        return Math.Round(Amount * (double)Sequence, 2);
    }

    public PrimaryAssignmentTier Copy()
    {
        return new PrimaryAssignmentTier(Code, Label, Amount, Archived, Sequence, Score, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
