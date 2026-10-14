namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh variance is in its life cycle.</summary>
public enum RefreshVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A refresh variance as the refresh team keeps it.</summary>
public sealed class RefreshVariance(string code, string label, double ratio, bool pinned, string location, int expiresAt, string country, string title, string category, string phone, RefreshVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public bool Pinned { get; set; } = pinned;
    public string Location { get; set; } = location;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Country { get; set; } = country;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Phone { get; set; } = phone;
    public RefreshVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a refresh variance in status new.</summary>
    public static RefreshVariance NewRefreshVariance(string code, string label)
    {
        return new RefreshVariance(code, label, 0.0, false, "", 0, "", "", "", "", RefreshVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshVarianceStatus next)
    {
        switch (Status)
        {
            case RefreshVarianceStatus.New:
                return next == RefreshVarianceStatus.Queued || next == RefreshVarianceStatus.Running;
            case RefreshVarianceStatus.Queued:
                switch (next)
                {
                    case RefreshVarianceStatus.Done:
                    case RefreshVarianceStatus.Failed:
                        return true;
                }
                return false;
            case RefreshVarianceStatus.Running:
                switch (next)
                {
                    case RefreshVarianceStatus.Done:
                    case RefreshVarianceStatus.Failed:
                        return true;
                }
                return false;
            case RefreshVarianceStatus.Done:
                switch (next)
                {
                    case RefreshVarianceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)ExpiresAt + Ratio, 2);
    }

    public RefreshVariance Copy()
    {
        return new RefreshVariance(
            Code,
            Label,
            Ratio,
            Pinned,
            Location,
            ExpiresAt,
            Country,
            Title,
            Category,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
