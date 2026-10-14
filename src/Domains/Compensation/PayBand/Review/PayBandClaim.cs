namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band claim is in its life cycle.</summary>
public enum PayBandClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pay band claim. Its code is unique within a tenant.</summary>
public sealed class PayBandClaim(string code, string label, int quantity, double hours, string ownerId, double minimum, bool approved, PayBandClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public double Hours { get; set; } = hours;
    public string OwnerId { get; set; } = ownerId;
    public double Minimum { get; set; } = minimum;
    public bool Approved { get; set; } = approved;
    public PayBandClaimStatus Status { get; set; } = status;

    /// <summary>Starts a pay band claim in status open.</summary>
    public static PayBandClaim NewPayBandClaim(string code, string label)
    {
        return new PayBandClaim(code, label, 0, 0.0, "", 0.0, false, PayBandClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandClaimStatus next)
    {
        switch (Status)
        {
            case PayBandClaimStatus.Open:
                switch (next)
                {
                    case PayBandClaimStatus.Reopened:
                        return true;
                }
                return false;
            case PayBandClaimStatus.InReview:
                switch (next)
                {
                    case PayBandClaimStatus.Resolved:
                    case PayBandClaimStatus.Reopened:
                        return true;
                }
                return false;
            case PayBandClaimStatus.Resolved:
                return next == PayBandClaimStatus.Reopened || next == PayBandClaimStatus.Archived;
            case PayBandClaimStatus.Reopened:
                return next == PayBandClaimStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PayBandClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pay band claim"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Minimum * Hours, 2);
    }

    public PayBandClaim Copy()
    {
        return new PayBandClaim(Code, Label, Quantity, Hours, OwnerId, Minimum, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
