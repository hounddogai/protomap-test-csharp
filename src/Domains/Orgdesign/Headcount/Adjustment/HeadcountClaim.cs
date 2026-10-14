namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount claim is in its life cycle.</summary>
public enum HeadcountClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A headcount claim as the headcount team keeps it.</summary>
public sealed class HeadcountClaim(string code, string title, int expiresAt, string channel, int capacity, double rate, HeadcountClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public int Capacity { get; set; } = capacity;
    public double Rate { get; set; } = rate;
    public HeadcountClaimStatus Status { get; set; } = status;

    /// <summary>Starts a headcount claim in status proposed.</summary>
    public static HeadcountClaim NewHeadcountClaim(string code, string title)
    {
        return new HeadcountClaim(code, title, 0, "", 0, 0.0, HeadcountClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountClaimStatus next)
    {
        switch (Status)
        {
            case HeadcountClaimStatus.Proposed:
                return next == HeadcountClaimStatus.Withdrawn;
            case HeadcountClaimStatus.Approved:
                switch (next)
                {
                    case HeadcountClaimStatus.Withdrawn:
                        return true;
                }
                return false;
            case HeadcountClaimStatus.Rejected:
                return next == HeadcountClaimStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(HeadcountClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)ExpiresAt * Rate, 2);
    }

    public HeadcountClaim Copy()
    {
        return new HeadcountClaim(Code, Title, ExpiresAt, Channel, Capacity, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
