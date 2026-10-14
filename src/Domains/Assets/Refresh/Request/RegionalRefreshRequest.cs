namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional refresh request is in its life cycle.</summary>
public enum RegionalRefreshRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A regional refresh request as the refresh team keeps it.</summary>
public sealed class RegionalRefreshRequest(string code, string label, int expiresAt, bool pinned, int windowDays, int createdAt, bool active, RegionalRefreshRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Pinned { get; set; } = pinned;
    public int WindowDays { get; set; } = windowDays;
    public int CreatedAt { get; set; } = createdAt;
    public bool Active { get; set; } = active;
    public RegionalRefreshRequestStatus Status { get; set; } = status;

    /// <summary>Starts a regional refresh request in status draft.</summary>
    public static RegionalRefreshRequest NewRegionalRefreshRequest(string code, string label)
    {
        return new RegionalRefreshRequest(code, label, 0, false, 0, 0, false, RegionalRefreshRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalRefreshRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalRefreshRequestStatus next)
    {
        switch (Status)
        {
            case RegionalRefreshRequestStatus.Draft:
                switch (next)
                {
                    case RegionalRefreshRequestStatus.Suspended:
                    case RegionalRefreshRequestStatus.Closed:
                        return true;
                }
                return false;
            case RegionalRefreshRequestStatus.Pending:
                switch (next)
                {
                    case RegionalRefreshRequestStatus.Suspended:
                        return true;
                }
                return false;
            case RegionalRefreshRequestStatus.Active:
                return next == RegionalRefreshRequestStatus.Suspended || next == RegionalRefreshRequestStatus.Closed;
            case RegionalRefreshRequestStatus.Suspended:
                switch (next)
                {
                    case RegionalRefreshRequestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalRefreshRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional refresh request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)CreatedAt * (double)WindowDays, 2);
    }

    public RegionalRefreshRequest Copy()
    {
        return new RegionalRefreshRequest(Code, Label, ExpiresAt, Pinned, WindowDays, CreatedAt, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
