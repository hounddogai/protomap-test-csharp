namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted attestation claim is in its life cycle.</summary>
public enum WeightedAttestationClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a weighted attestation claim and its current status.</summary>
public sealed class WeightedAttestationClaim(string code, string title, string managerId, string skill, string reviewer, double score, WeightedAttestationClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Skill { get; set; } = skill;
    public string Reviewer { get; set; } = reviewer;
    public double Score { get; set; } = score;
    public WeightedAttestationClaimStatus Status { get; set; } = status;

    /// <summary>Starts a weighted attestation claim in status draft.</summary>
    public static WeightedAttestationClaim NewWeightedAttestationClaim(string code, string title)
    {
        return new WeightedAttestationClaim(code, title, "", "", "", 0.0, WeightedAttestationClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAttestationClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAttestationClaimStatus next)
    {
        switch (Status)
        {
            case WeightedAttestationClaimStatus.Draft:
                return next == WeightedAttestationClaimStatus.Closed;
            case WeightedAttestationClaimStatus.Pending:
                return next == WeightedAttestationClaimStatus.Suspended || next == WeightedAttestationClaimStatus.Closed;
            case WeightedAttestationClaimStatus.Active:
                switch (next)
                {
                    case WeightedAttestationClaimStatus.Suspended:
                        return true;
                }
                return false;
            case WeightedAttestationClaimStatus.Suspended:
                switch (next)
                {
                    case WeightedAttestationClaimStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedAttestationClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted attestation claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public WeightedAttestationClaim Copy()
    {
        return new WeightedAttestationClaim(Code, Title, ManagerId, Skill, Reviewer, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
