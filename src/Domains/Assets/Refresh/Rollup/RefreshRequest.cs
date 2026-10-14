namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh request is in its life cycle.</summary>
public enum RefreshRequestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A refresh request as the refresh team keeps it.</summary>
public sealed class RefreshRequest(string code, string title, double rating, string label, int version, RefreshRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public RefreshRequestStatus Status { get; set; } = status;

    /// <summary>Starts a refresh request in status proposed.</summary>
    public static RefreshRequest NewRefreshRequest(string code, string title)
    {
        return new RefreshRequest(code, title, 0.0, "", 0, RefreshRequestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshRequestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshRequestStatus next)
    {
        switch (Status)
        {
            case RefreshRequestStatus.Proposed:
                switch (next)
                {
                    case RefreshRequestStatus.Approved:
                        return true;
                }
                return false;
            case RefreshRequestStatus.Approved:
                switch (next)
                {
                    case RefreshRequestStatus.Rejected:
                    case RefreshRequestStatus.Withdrawn:
                        return true;
                }
                return false;
            case RefreshRequestStatus.Rejected:
                switch (next)
                {
                    case RefreshRequestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rating - (double)Version, 2);
    }

    public RefreshRequest Copy()
    {
        return new RefreshRequest(Code, Title, Rating, Label, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
