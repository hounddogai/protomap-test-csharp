namespace Acme.Workforce.Assets.Assignment.AssignmentClaim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered assignment claim is in its life cycle.</summary>
public enum TieredAssignmentClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered assignment claim. Its code is unique within a tenant.</summary>
public sealed class TieredAssignmentClaim(string code, string name, double hours, int endsAt, double threshold, TieredAssignmentClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public int EndsAt { get; set; } = endsAt;
    public double Threshold { get; set; } = threshold;
    public TieredAssignmentClaimStatus Status { get; set; } = status;

    /// <summary>Starts a tiered assignment claim in status draft.</summary>
    public static TieredAssignmentClaim NewTieredAssignmentClaim(string code, string name)
    {
        return new TieredAssignmentClaim(code, name, 0.0, 0, 0.0, TieredAssignmentClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAssignmentClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAssignmentClaimStatus next)
    {
        switch (Status)
        {
            case TieredAssignmentClaimStatus.Draft:
                return next == TieredAssignmentClaimStatus.Suspended || next == TieredAssignmentClaimStatus.Closed;
            case TieredAssignmentClaimStatus.Pending:
                return next == TieredAssignmentClaimStatus.Suspended;
            case TieredAssignmentClaimStatus.Active:
                return next == TieredAssignmentClaimStatus.Suspended || next == TieredAssignmentClaimStatus.Closed;
            case TieredAssignmentClaimStatus.Suspended:
                switch (next)
                {
                    case TieredAssignmentClaimStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredAssignmentClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered assignment claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedValue()
    {
        return Math.Round(Threshold + (double)EndsAt, 2);
    }

    public TieredAssignmentClaim Copy()
    {
        return new TieredAssignmentClaim(Code, Name, Hours, EndsAt, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
