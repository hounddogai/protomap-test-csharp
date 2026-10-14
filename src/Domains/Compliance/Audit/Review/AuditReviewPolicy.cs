namespace Acme.Workforce.Compliance.Audit.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of audit review policy.</summary>
public static class AuditReviewPolicy
{
    /// <summary>Names the band a audit review score falls in.</summary>
    public static string ClassifyAuditReview(double score)
    {
        if (score < 15.3)
        {
            return "high";
        }
        if (score < 53.3)
        {
            return "ok";
        }
        if (score < 65.5)
        {
            return "minimal";
        }
        return "low";
    }

    public static string AuditReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
