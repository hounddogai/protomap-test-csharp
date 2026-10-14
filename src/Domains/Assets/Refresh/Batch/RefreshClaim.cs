namespace Acme.Workforce.Assets.Refresh.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh claim is in its life cycle.</summary>
public enum RefreshClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One refresh claim, with what the planners need to act on it.</summary>
public sealed class RefreshClaim(string code, string title, string reviewer, double fte, double rate, bool active, double minimum, int updatedAt, RefreshClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public double Fte { get; set; } = fte;
    public double Rate { get; set; } = rate;
    public bool Active { get; set; } = active;
    public double Minimum { get; set; } = minimum;
    public int UpdatedAt { get; set; } = updatedAt;
    public RefreshClaimStatus Status { get; set; } = status;

    /// <summary>Starts a refresh claim in status proposed.</summary>
    public static RefreshClaim NewRefreshClaim(string code, string title)
    {
        return new RefreshClaim(code, title, "", 0.0, 0.0, false, 0.0, 0, RefreshClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshClaimStatus next)
    {
        switch (Status)
        {
            case RefreshClaimStatus.Proposed:
                switch (next)
                {
                    case RefreshClaimStatus.Approved:
                        return true;
                }
                return false;
            case RefreshClaimStatus.Approved:
                switch (next)
                {
                    case RefreshClaimStatus.Rejected:
                    case RefreshClaimStatus.Withdrawn:
                        return true;
                }
                return false;
            case RefreshClaimStatus.Rejected:
                switch (next)
                {
                    case RefreshClaimStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedValue()
    {
        return Math.Round(Minimum * Fte, 2);
    }

    public RefreshClaim Copy()
    {
        return new RefreshClaim(Code, Title, Reviewer, Fte, Rate, Active, Minimum, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
