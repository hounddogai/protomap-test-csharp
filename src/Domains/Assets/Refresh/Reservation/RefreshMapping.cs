namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh mapping is in its life cycle.</summary>
public enum RefreshMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One refresh mapping, with what the planners need to act on it.</summary>
public sealed class RefreshMapping(string code, string label, int batchSize, int priority, string title, RefreshMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public int Priority { get; set; } = priority;
    public string Title { get; set; } = title;
    public RefreshMappingStatus Status { get; set; } = status;

    /// <summary>Starts a refresh mapping in status idle.</summary>
    public static RefreshMapping NewRefreshMapping(string code, string label)
    {
        return new RefreshMapping(code, label, 0, 0, "", RefreshMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshMappingStatus next)
    {
        switch (Status)
        {
            case RefreshMappingStatus.Idle:
                switch (next)
                {
                    case RefreshMappingStatus.Draining:
                        return true;
                }
                return false;
            case RefreshMappingStatus.Warming:
                return next == RefreshMappingStatus.Draining;
            case RefreshMappingStatus.Ready:
                return next == RefreshMappingStatus.Draining;
            case RefreshMappingStatus.Draining:
                switch (next)
                {
                    case RefreshMappingStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Priority - (double)BatchSize, 2);
    }

    public RefreshMapping Copy()
    {
        return new RefreshMapping(Code, Label, BatchSize, Priority, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
