namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy claim is in its life cycle.</summary>
public enum PolicyClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A policy claim. Its code is unique within a tenant.</summary>
public sealed class PolicyClaim(string code, string title, double weight, string category, string team, double cost, double rating, string unit, PolicyClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public string Category { get; set; } = category;
    public string Team { get; set; } = team;
    public double Cost { get; set; } = cost;
    public double Rating { get; set; } = rating;
    public string Unit { get; set; } = unit;
    public PolicyClaimStatus Status { get; set; } = status;

    /// <summary>Starts a policy claim in status draft.</summary>
    public static PolicyClaim NewPolicyClaim(string code, string title)
    {
        return new PolicyClaim(code, title, 0.0, "", "", 0.0, 0.0, "", PolicyClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyClaimStatus next)
    {
        switch (Status)
        {
            case PolicyClaimStatus.Draft:
                return next == PolicyClaimStatus.Suspended || next == PolicyClaimStatus.Closed;
            case PolicyClaimStatus.Pending:
                switch (next)
                {
                    case PolicyClaimStatus.Active:
                    case PolicyClaimStatus.Closed:
                        return true;
                }
                return false;
            case PolicyClaimStatus.Active:
                return next == PolicyClaimStatus.Suspended || next == PolicyClaimStatus.Closed;
            case PolicyClaimStatus.Suspended:
                return next == PolicyClaimStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PolicyClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetLoad()
    {
        return Math.Round(Cost + Weight, 2);
    }

    public PolicyClaim Copy()
    {
        return new PolicyClaim(Code, Title, Weight, Category, Team, Cost, Rating, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
