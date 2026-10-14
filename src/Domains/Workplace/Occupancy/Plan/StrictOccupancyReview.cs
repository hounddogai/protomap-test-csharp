namespace Acme.Workforce.Workplace.Occupancy.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict occupancy review is in its life cycle.</summary>
public enum StrictOccupancyReviewStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One strict occupancy review, with what the planners need to act on it.</summary>
public sealed class StrictOccupancyReview(string code, string title, double ratio, string skill, double rate, string costCenter, StrictOccupancyReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Skill { get; set; } = skill;
    public double Rate { get; set; } = rate;
    public string CostCenter { get; set; } = costCenter;
    public StrictOccupancyReviewStatus Status { get; set; } = status;

    /// <summary>Starts a strict occupancy review in status created.</summary>
    public static StrictOccupancyReview NewStrictOccupancyReview(string code, string title)
    {
        return new StrictOccupancyReview(code, title, 0.0, "", 0.0, "", StrictOccupancyReviewStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictOccupancyReviewStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictOccupancyReviewStatus next)
    {
        switch (Status)
        {
            case StrictOccupancyReviewStatus.Created:
                switch (next)
                {
                    case StrictOccupancyReviewStatus.Published:
                        return true;
                }
                return false;
            case StrictOccupancyReviewStatus.Validated:
                switch (next)
                {
                    case StrictOccupancyReviewStatus.Published:
                    case StrictOccupancyReviewStatus.Retired:
                        return true;
                }
                return false;
            case StrictOccupancyReviewStatus.Published:
                switch (next)
                {
                    case StrictOccupancyReviewStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictOccupancyReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict occupancy review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"strict occupancy review"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Ratio * Rate, 2);
    }

    public StrictOccupancyReview Copy()
    {
        return new StrictOccupancyReview(Code, Title, Ratio, Skill, Rate, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
