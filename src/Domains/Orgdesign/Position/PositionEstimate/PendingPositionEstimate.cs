namespace Acme.Workforce.Orgdesign.Position.PositionEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending position estimate is in its life cycle.</summary>
public enum PendingPositionEstimateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A pending position estimate. Its code is unique within a tenant.</summary>
public sealed class PendingPositionEstimate(string code, string title, string contactEmail, string managerId, int retries, int windowDays, string location, double threshold, PendingPositionEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public string ManagerId { get; set; } = managerId;
    public int Retries { get; set; } = retries;
    public int WindowDays { get; set; } = windowDays;
    public string Location { get; set; } = location;
    public double Threshold { get; set; } = threshold;
    public PendingPositionEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a pending position estimate in status open.</summary>
    public static PendingPositionEstimate NewPendingPositionEstimate(string code, string title)
    {
        return new PendingPositionEstimate(code, title, "", "", 0, 0, "", 0.0, PendingPositionEstimateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPositionEstimateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPositionEstimateStatus next)
    {
        switch (Status)
        {
            case PendingPositionEstimateStatus.Open:
                switch (next)
                {
                    case PendingPositionEstimateStatus.InReview:
                        return true;
                }
                return false;
            case PendingPositionEstimateStatus.InReview:
                switch (next)
                {
                    case PendingPositionEstimateStatus.Archived:
                        return true;
                }
                return false;
            case PendingPositionEstimateStatus.Resolved:
                switch (next)
                {
                    case PendingPositionEstimateStatus.Archived:
                        return true;
                }
                return false;
            case PendingPositionEstimateStatus.Reopened:
                switch (next)
                {
                    case PendingPositionEstimateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPositionEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending position estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)WindowDays * Threshold, 2);
    }

    public PendingPositionEstimate Copy()
    {
        return new PendingPositionEstimate(
            Code,
            Title,
            ContactEmail,
            ManagerId,
            Retries,
            WindowDays,
            Location,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
