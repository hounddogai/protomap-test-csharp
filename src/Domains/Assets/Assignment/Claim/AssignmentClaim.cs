namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment claim is in its life cycle.</summary>
public enum AssignmentClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A assignment claim. Its code is unique within a tenant.</summary>
public sealed class AssignmentClaim(string code, string name, string reviewer, double rating, double threshold, AssignmentClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public double Rating { get; set; } = rating;
    public double Threshold { get; set; } = threshold;
    public AssignmentClaimStatus Status { get; set; } = status;

    /// <summary>Starts a assignment claim in status draft.</summary>
    public static AssignmentClaim NewAssignmentClaim(string code, string name)
    {
        return new AssignmentClaim(code, name, "", 0.0, 0.0, AssignmentClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentClaimStatus next)
    {
        switch (Status)
        {
            case AssignmentClaimStatus.Draft:
                switch (next)
                {
                    case AssignmentClaimStatus.Pending:
                    case AssignmentClaimStatus.Suspended:
                        return true;
                }
                return false;
            case AssignmentClaimStatus.Pending:
                switch (next)
                {
                    case AssignmentClaimStatus.Suspended:
                        return true;
                }
                return false;
            case AssignmentClaimStatus.Active:
                return next == AssignmentClaimStatus.Suspended;
            case AssignmentClaimStatus.Suspended:
                switch (next)
                {
                    case AssignmentClaimStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"assignment claim"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Rating + Threshold, 2);
    }

    public AssignmentClaim Copy()
    {
        return new AssignmentClaim(Code, Name, Reviewer, Rating, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
