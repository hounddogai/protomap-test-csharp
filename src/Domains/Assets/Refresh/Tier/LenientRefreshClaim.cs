namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient refresh claim is in its life cycle.</summary>
public enum LenientRefreshClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient refresh claim as the refresh team keeps it.</summary>
public sealed class LenientRefreshClaim(string code, string label, double fte, string department, string source, string timezone, string checksum, int startsAt, LenientRefreshClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public string Department { get; set; } = department;
    public string Source { get; set; } = source;
    public string Timezone { get; set; } = timezone;
    public string Checksum { get; set; } = checksum;
    public int StartsAt { get; set; } = startsAt;
    public LenientRefreshClaimStatus Status { get; set; } = status;

    /// <summary>Starts a lenient refresh claim in status draft.</summary>
    public static LenientRefreshClaim NewLenientRefreshClaim(string code, string label)
    {
        return new LenientRefreshClaim(code, label, 0.0, "", "", "", "", 0, LenientRefreshClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientRefreshClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientRefreshClaimStatus next)
    {
        switch (Status)
        {
            case LenientRefreshClaimStatus.Draft:
                switch (next)
                {
                    case LenientRefreshClaimStatus.Pending:
                    case LenientRefreshClaimStatus.Active:
                        return true;
                }
                return false;
            case LenientRefreshClaimStatus.Pending:
                switch (next)
                {
                    case LenientRefreshClaimStatus.Active:
                    case LenientRefreshClaimStatus.Closed:
                        return true;
                }
                return false;
            case LenientRefreshClaimStatus.Active:
                switch (next)
                {
                    case LenientRefreshClaimStatus.Suspended:
                        return true;
                }
                return false;
            case LenientRefreshClaimStatus.Suspended:
                switch (next)
                {
                    case LenientRefreshClaimStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientRefreshClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient refresh claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Fte * (double)StartsAt, 2);
    }

    public LenientRefreshClaim Copy()
    {
        return new LenientRefreshClaim(Code, Label, Fte, Department, Source, Timezone, Checksum, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
