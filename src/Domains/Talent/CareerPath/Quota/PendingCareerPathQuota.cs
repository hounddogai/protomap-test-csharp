namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending career path quota is in its life cycle.</summary>
public enum PendingCareerPathQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a pending career path quota and its current status.</summary>
public sealed class PendingCareerPathQuota(string code, string title, double score, double budget, double ceiling, double weight, string reviewer, PendingCareerPathQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public double Budget { get; set; } = budget;
    public double Ceiling { get; set; } = ceiling;
    public double Weight { get; set; } = weight;
    public string Reviewer { get; set; } = reviewer;
    public PendingCareerPathQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a pending career path quota in status proposed.</summary>
    public static PendingCareerPathQuota NewPendingCareerPathQuota(string code, string title)
    {
        return new PendingCareerPathQuota(
            code,
            title,
            0.0,
            0.0,
            0.0,
            0.0,
            "",
            PendingCareerPathQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingCareerPathQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingCareerPathQuotaStatus next)
    {
        switch (Status)
        {
            case PendingCareerPathQuotaStatus.Proposed:
                return next == PendingCareerPathQuotaStatus.Rejected;
            case PendingCareerPathQuotaStatus.Approved:
                return next == PendingCareerPathQuotaStatus.Rejected || next == PendingCareerPathQuotaStatus.Withdrawn;
            case PendingCareerPathQuotaStatus.Rejected:
                switch (next)
                {
                    case PendingCareerPathQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingCareerPathQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending career path quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending career path quota"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Ceiling - Budget, 2);
    }

    public PendingCareerPathQuota Copy()
    {
        return new PendingCareerPathQuota(Code, Title, Score, Budget, Ceiling, Weight, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
