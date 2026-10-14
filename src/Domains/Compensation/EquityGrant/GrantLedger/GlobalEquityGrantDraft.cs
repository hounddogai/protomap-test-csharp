namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global equity grant draft is in its life cycle.</summary>
public enum GlobalEquityGrantDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A global equity grant draft as the equity grant team keeps it.</summary>
public sealed class GlobalEquityGrantDraft(string code, string title, int retries, string region, string team, GlobalEquityGrantDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string Region { get; set; } = region;
    public string Team { get; set; } = team;
    public GlobalEquityGrantDraftStatus Status { get; set; } = status;

    /// <summary>Starts a global equity grant draft in status idle.</summary>
    public static GlobalEquityGrantDraft NewGlobalEquityGrantDraft(string code, string title)
    {
        return new GlobalEquityGrantDraft(code, title, 0, "", "", GlobalEquityGrantDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalEquityGrantDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalEquityGrantDraftStatus next)
    {
        switch (Status)
        {
            case GlobalEquityGrantDraftStatus.Idle:
                return next == GlobalEquityGrantDraftStatus.Ready;
            case GlobalEquityGrantDraftStatus.Warming:
                return next == GlobalEquityGrantDraftStatus.Draining || next == GlobalEquityGrantDraftStatus.Stopped;
            case GlobalEquityGrantDraftStatus.Ready:
                return next == GlobalEquityGrantDraftStatus.Draining;
            case GlobalEquityGrantDraftStatus.Draining:
                return next == GlobalEquityGrantDraftStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalEquityGrantDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global equity grant draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global equity grant draft"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public GlobalEquityGrantDraft Copy()
    {
        return new GlobalEquityGrantDraft(Code, Title, Retries, Region, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
