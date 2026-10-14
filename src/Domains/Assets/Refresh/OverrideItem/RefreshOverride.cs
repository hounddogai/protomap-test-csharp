namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh override is in its life cycle.</summary>
public enum RefreshOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A refresh override as the refresh team keeps it.</summary>
public sealed class RefreshOverride(string code, string label, bool pinned, string phone, int sequence, double maximum, double rate, double margin, int version, RefreshOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string Phone { get; set; } = phone;
    public int Sequence { get; set; } = sequence;
    public double Maximum { get; set; } = maximum;
    public double Rate { get; set; } = rate;
    public double Margin { get; set; } = margin;
    public int Version { get; set; } = version;
    public RefreshOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a refresh override in status new.</summary>
    public static RefreshOverride NewRefreshOverride(string code, string label)
    {
        return new RefreshOverride(code, label, false, "", 0, 0.0, 0.0, 0.0, 0, RefreshOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshOverrideStatus next)
    {
        switch (Status)
        {
            case RefreshOverrideStatus.New:
                return next == RefreshOverrideStatus.Queued;
            case RefreshOverrideStatus.Queued:
                return next == RefreshOverrideStatus.Running;
            case RefreshOverrideStatus.Running:
                return next == RefreshOverrideStatus.Failed;
            case RefreshOverrideStatus.Done:
                switch (next)
                {
                    case RefreshOverrideStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"refresh override"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Version + Maximum, 2);
    }

    public RefreshOverride Copy()
    {
        return new RefreshOverride(Code, Label, Pinned, Phone, Sequence, Maximum, Rate, Margin, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
