namespace Acme.Workforce.Payrollops.Garnishment.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment review is in its life cycle.</summary>
public enum GarnishmentReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a garnishment review and its current status.</summary>
public sealed class GarnishmentReview(string code, string title, string managerId, string channel, int version, double cost, GarnishmentReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public int Version { get; set; } = version;
    public double Cost { get; set; } = cost;
    public GarnishmentReviewStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment review in status planned.</summary>
    public static GarnishmentReview NewGarnishmentReview(string code, string title)
    {
        return new GarnishmentReview(code, title, "", "", 0, 0.0, GarnishmentReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentReviewStatus next)
    {
        switch (Status)
        {
            case GarnishmentReviewStatus.Planned:
                switch (next)
                {
                    case GarnishmentReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case GarnishmentReviewStatus.Scheduled:
                switch (next)
                {
                    case GarnishmentReviewStatus.Completed:
                    case GarnishmentReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case GarnishmentReviewStatus.InProgress:
                return next == GarnishmentReviewStatus.Completed || next == GarnishmentReviewStatus.Cancelled;
            case GarnishmentReviewStatus.Completed:
                switch (next)
                {
                    case GarnishmentReviewStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedValue()
    {
        return Math.Round(Cost + (double)Version, 2);
    }

    public GarnishmentReview Copy()
    {
        return new GarnishmentReview(Code, Title, ManagerId, Channel, Version, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
