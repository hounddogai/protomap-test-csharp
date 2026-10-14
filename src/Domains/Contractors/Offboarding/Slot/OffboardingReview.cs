namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding review is in its life cycle.</summary>
public enum OffboardingReviewStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A offboarding review as the offboarding team keeps it.</summary>
public sealed class OffboardingReview(string code, string label, double amount, int attempts, string country, double fte, string segment, double weight, string category, OffboardingReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public int Attempts { get; set; } = attempts;
    public string Country { get; set; } = country;
    public double Fte { get; set; } = fte;
    public string Segment { get; set; } = segment;
    public double Weight { get; set; } = weight;
    public string Category { get; set; } = category;
    public OffboardingReviewStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding review in status requested.</summary>
    public static OffboardingReview NewOffboardingReview(string code, string label)
    {
        return new OffboardingReview(code, label, 0.0, 0, "", 0.0, "", 0.0, "", OffboardingReviewStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingReviewStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingReviewStatus next)
    {
        switch (Status)
        {
            case OffboardingReviewStatus.Requested:
                return next == OffboardingReviewStatus.Revoked || next == OffboardingReviewStatus.Expired;
            case OffboardingReviewStatus.Granted:
                return next == OffboardingReviewStatus.Revoked || next == OffboardingReviewStatus.Expired;
            case OffboardingReviewStatus.Revoked:
                switch (next)
                {
                    case OffboardingReviewStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Fte - Amount, 2);
    }

    public OffboardingReview Copy()
    {
        return new OffboardingReview(Code, Label, Amount, Attempts, Country, Fte, Segment, Weight, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
