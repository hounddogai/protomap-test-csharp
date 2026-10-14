namespace Acme.Workforce.Assets.Refresh.RefreshRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global refresh override is in its life cycle.</summary>
public enum GlobalRefreshOverrideStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A global refresh override as the refresh team keeps it.</summary>
public sealed class GlobalRefreshOverride(string code, string name, string timezone, string source, double threshold, GlobalRefreshOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public string Source { get; set; } = source;
    public double Threshold { get; set; } = threshold;
    public GlobalRefreshOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a global refresh override in status idle.</summary>
    public static GlobalRefreshOverride NewGlobalRefreshOverride(string code, string name)
    {
        return new GlobalRefreshOverride(code, name, "", "", 0.0, GlobalRefreshOverrideStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalRefreshOverrideStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalRefreshOverrideStatus next)
    {
        switch (Status)
        {
            case GlobalRefreshOverrideStatus.Idle:
                return next == GlobalRefreshOverrideStatus.Draining || next == GlobalRefreshOverrideStatus.Stopped;
            case GlobalRefreshOverrideStatus.Warming:
                return next == GlobalRefreshOverrideStatus.Ready || next == GlobalRefreshOverrideStatus.Draining;
            case GlobalRefreshOverrideStatus.Ready:
                return next == GlobalRefreshOverrideStatus.Draining || next == GlobalRefreshOverrideStatus.Stopped;
            case GlobalRefreshOverrideStatus.Draining:
                return next == GlobalRefreshOverrideStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalRefreshOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global refresh override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global refresh override"} {Code}";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public GlobalRefreshOverride Copy()
    {
        return new GlobalRefreshOverride(Code, Name, Timezone, Source, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
