namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted rate request is in its life cycle.</summary>
public enum WeightedRateRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A weighted rate request. Its code is unique within a tenant.</summary>
public sealed class WeightedRateRequest(string code, string title, string location, int graceDays, int attempts, string reviewer, string description, double margin, bool active, WeightedRateRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public int GraceDays { get; set; } = graceDays;
    public int Attempts { get; set; } = attempts;
    public string Reviewer { get; set; } = reviewer;
    public string Description { get; set; } = description;
    public double Margin { get; set; } = margin;
    public bool Active { get; set; } = active;
    public WeightedRateRequestStatus Status { get; set; } = status;

    /// <summary>Starts a weighted rate request in status draft.</summary>
    public static WeightedRateRequest NewWeightedRateRequest(string code, string title)
    {
        return new WeightedRateRequest(code, title, "", 0, 0, "", "", 0.0, false, WeightedRateRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRateRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRateRequestStatus next)
    {
        switch (Status)
        {
            case WeightedRateRequestStatus.Draft:
                return next == WeightedRateRequestStatus.Suspended;
            case WeightedRateRequestStatus.Pending:
                return next == WeightedRateRequestStatus.Active;
            case WeightedRateRequestStatus.Active:
                return next == WeightedRateRequestStatus.Closed;
            case WeightedRateRequestStatus.Suspended:
                return next == WeightedRateRequestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(WeightedRateRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted rate request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Margin - (double)GraceDays, 2);
    }

    public WeightedRateRequest Copy()
    {
        return new WeightedRateRequest(
            Code,
            Title,
            Location,
            GraceDays,
            Attempts,
            Reviewer,
            Description,
            Margin,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
