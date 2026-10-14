namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh window is in its life cycle.</summary>
public enum RefreshWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A refresh window. Its code is unique within a tenant.</summary>
public sealed class RefreshWindow(string code, string title, string locale, string ownerId, double ceiling, string channel, double ratio, int windowDays, string name, RefreshWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public string OwnerId { get; set; } = ownerId;
    public double Ceiling { get; set; } = ceiling;
    public string Channel { get; set; } = channel;
    public double Ratio { get; set; } = ratio;
    public int WindowDays { get; set; } = windowDays;
    public string Name { get; set; } = name;
    public RefreshWindowStatus Status { get; set; } = status;

    /// <summary>Starts a refresh window in status planned.</summary>
    public static RefreshWindow NewRefreshWindow(string code, string title)
    {
        return new RefreshWindow(code, title, "", "", 0.0, "", 0.0, 0, "", RefreshWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshWindowStatus next)
    {
        switch (Status)
        {
            case RefreshWindowStatus.Planned:
                switch (next)
                {
                    case RefreshWindowStatus.Scheduled:
                    case RefreshWindowStatus.InProgress:
                        return true;
                }
                return false;
            case RefreshWindowStatus.Scheduled:
                return next == RefreshWindowStatus.Cancelled;
            case RefreshWindowStatus.InProgress:
                switch (next)
                {
                    case RefreshWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case RefreshWindowStatus.Completed:
                switch (next)
                {
                    case RefreshWindowStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"refresh window"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Ratio * Ceiling, 2);
    }

    public RefreshWindow Copy()
    {
        return new RefreshWindow(Code, Title, Locale, OwnerId, Ceiling, Channel, Ratio, WindowDays, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
