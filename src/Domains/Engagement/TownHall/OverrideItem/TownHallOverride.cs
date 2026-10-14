namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall override is in its life cycle.</summary>
public enum TownHallOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A town hall override as the town hall team keeps it.</summary>
public sealed class TownHallOverride(string code, string title, double rating, string grade, int batchSize, TownHallOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Grade { get; set; } = grade;
    public int BatchSize { get; set; } = batchSize;
    public TownHallOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a town hall override in status requested.</summary>
    public static TownHallOverride NewTownHallOverride(string code, string title)
    {
        return new TownHallOverride(code, title, 0.0, "", 0, TownHallOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallOverrideStatus next)
    {
        switch (Status)
        {
            case TownHallOverrideStatus.Requested:
                switch (next)
                {
                    case TownHallOverrideStatus.Revoked:
                        return true;
                }
                return false;
            case TownHallOverrideStatus.Granted:
                switch (next)
                {
                    case TownHallOverrideStatus.Expired:
                        return true;
                }
                return false;
            case TownHallOverrideStatus.Revoked:
                switch (next)
                {
                    case TownHallOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetVolume()
    {
        return Math.Round(Rating - (double)BatchSize, 2);
    }

    public TownHallOverride Copy()
    {
        return new TownHallOverride(Code, Title, Rating, Grade, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
