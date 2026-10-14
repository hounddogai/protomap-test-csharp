namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor review is in its life cycle.</summary>
public enum VendorReviewStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a vendor review and its current status.</summary>
public sealed class VendorReview(string code, string title, double rating, string currency, int createdAt, int capacity, string badge, VendorReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Currency { get; set; } = currency;
    public int CreatedAt { get; set; } = createdAt;
    public int Capacity { get; set; } = capacity;
    public string Badge { get; set; } = badge;
    public VendorReviewStatus Status { get; set; } = status;

    /// <summary>Starts a vendor review in status created.</summary>
    public static VendorReview NewVendorReview(string code, string title)
    {
        return new VendorReview(code, title, 0.0, "", 0, 0, "", VendorReviewStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorReviewStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorReviewStatus next)
    {
        switch (Status)
        {
            case VendorReviewStatus.Created:
                switch (next)
                {
                    case VendorReviewStatus.Validated:
                    case VendorReviewStatus.Retired:
                        return true;
                }
                return false;
            case VendorReviewStatus.Validated:
                switch (next)
                {
                    case VendorReviewStatus.Retired:
                        return true;
                }
                return false;
            case VendorReviewStatus.Published:
                return next == VendorReviewStatus.Retired;
        }
        return false;
    }

    public void MoveTo(VendorReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"vendor review"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)CreatedAt - (double)Capacity, 2);
    }

    public VendorReview Copy()
    {
        return new VendorReview(Code, Title, Rating, Currency, CreatedAt, Capacity, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
