namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk claim is in its life cycle.</summary>
public enum DeskClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a desk claim and its current status.</summary>
public sealed class DeskClaim(string code, string label, int createdAt, int expiresAt, double rate, string department, double fte, DeskClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Rate { get; set; } = rate;
    public string Department { get; set; } = department;
    public double Fte { get; set; } = fte;
    public DeskClaimStatus Status { get; set; } = status;

    /// <summary>Starts a desk claim in status open.</summary>
    public static DeskClaim NewDeskClaim(string code, string label)
    {
        return new DeskClaim(code, label, 0, 0, 0.0, "", 0.0, DeskClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskClaimStatus next)
    {
        switch (Status)
        {
            case DeskClaimStatus.Open:
                switch (next)
                {
                    case DeskClaimStatus.InReview:
                    case DeskClaimStatus.Reopened:
                        return true;
                }
                return false;
            case DeskClaimStatus.InReview:
                return next == DeskClaimStatus.Resolved;
            case DeskClaimStatus.Resolved:
                return next == DeskClaimStatus.Reopened;
            case DeskClaimStatus.Reopened:
                switch (next)
                {
                    case DeskClaimStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetTotal()
    {
        return Math.Round((double)ExpiresAt + (double)CreatedAt, 2);
    }

    public DeskClaim Copy()
    {
        return new DeskClaim(Code, Label, CreatedAt, ExpiresAt, Rate, Department, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
