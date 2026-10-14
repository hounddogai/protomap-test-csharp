namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh profile is in its life cycle.</summary>
public enum RefreshProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A refresh profile as the refresh team keeps it.</summary>
public sealed class RefreshProfile(string code, string title, int durationMinutes, string grade, double rating, RefreshProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Grade { get; set; } = grade;
    public double Rating { get; set; } = rating;
    public RefreshProfileStatus Status { get; set; } = status;

    /// <summary>Starts a refresh profile in status draft.</summary>
    public static RefreshProfile NewRefreshProfile(string code, string title)
    {
        return new RefreshProfile(code, title, 0, "", 0.0, RefreshProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshProfileStatus next)
    {
        switch (Status)
        {
            case RefreshProfileStatus.Draft:
                switch (next)
                {
                    case RefreshProfileStatus.Active:
                        return true;
                }
                return false;
            case RefreshProfileStatus.Pending:
                return next == RefreshProfileStatus.Suspended || next == RefreshProfileStatus.Closed;
            case RefreshProfileStatus.Active:
                return next == RefreshProfileStatus.Closed;
            case RefreshProfileStatus.Suspended:
                switch (next)
                {
                    case RefreshProfileStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"refresh profile"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)DurationMinutes + Rating, 2);
    }

    public RefreshProfile Copy()
    {
        return new RefreshProfile(Code, Title, DurationMinutes, Grade, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
