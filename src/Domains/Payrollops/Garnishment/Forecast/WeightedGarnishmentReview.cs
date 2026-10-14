namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted garnishment review is in its life cycle.</summary>
public enum WeightedGarnishmentReviewStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A weighted garnishment review as the garnishment team keeps it.</summary>
public sealed class WeightedGarnishmentReview(string code, string title, bool pinned, double fte, bool visible, string timezone, double budget, WeightedGarnishmentReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public double Fte { get; set; } = fte;
    public bool Visible { get; set; } = visible;
    public string Timezone { get; set; } = timezone;
    public double Budget { get; set; } = budget;
    public WeightedGarnishmentReviewStatus Status { get; set; } = status;

    /// <summary>Starts a weighted garnishment review in status created.</summary>
    public static WeightedGarnishmentReview NewWeightedGarnishmentReview(string code, string title)
    {
        return new WeightedGarnishmentReview(
            code,
            title,
            false,
            0.0,
            false,
            "",
            0.0,
            WeightedGarnishmentReviewStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedGarnishmentReviewStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedGarnishmentReviewStatus next)
    {
        switch (Status)
        {
            case WeightedGarnishmentReviewStatus.Created:
                switch (next)
                {
                    case WeightedGarnishmentReviewStatus.Validated:
                    case WeightedGarnishmentReviewStatus.Retired:
                        return true;
                }
                return false;
            case WeightedGarnishmentReviewStatus.Validated:
                switch (next)
                {
                    case WeightedGarnishmentReviewStatus.Published:
                        return true;
                }
                return false;
            case WeightedGarnishmentReviewStatus.Published:
                switch (next)
                {
                    case WeightedGarnishmentReviewStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedGarnishmentReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted garnishment review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Budget + Fte, 2);
    }

    public WeightedGarnishmentReview Copy()
    {
        return new WeightedGarnishmentReview(Code, Title, Pinned, Fte, Visible, Timezone, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
