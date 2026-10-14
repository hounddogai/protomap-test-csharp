namespace Acme.Workforce.Talent.Nomination.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination claim is in its life cycle.</summary>
public enum NominationClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a nomination claim and its current status.</summary>
public sealed class NominationClaim(string code, string title, int startsAt, bool pinned, string location, int attempts, NominationClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public bool Pinned { get; set; } = pinned;
    public string Location { get; set; } = location;
    public int Attempts { get; set; } = attempts;
    public NominationClaimStatus Status { get; set; } = status;

    /// <summary>Starts a nomination claim in status proposed.</summary>
    public static NominationClaim NewNominationClaim(string code, string title)
    {
        return new NominationClaim(code, title, 0, false, "", 0, NominationClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationClaimStatus next)
    {
        switch (Status)
        {
            case NominationClaimStatus.Proposed:
                switch (next)
                {
                    case NominationClaimStatus.Withdrawn:
                        return true;
                }
                return false;
            case NominationClaimStatus.Approved:
                switch (next)
                {
                    case NominationClaimStatus.Rejected:
                        return true;
                }
                return false;
            case NominationClaimStatus.Rejected:
                return next == NominationClaimStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(NominationClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"nomination claim"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Attempts * (double)StartsAt, 2);
    }

    public NominationClaim Copy()
    {
        return new NominationClaim(Code, Title, StartsAt, Pinned, Location, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
