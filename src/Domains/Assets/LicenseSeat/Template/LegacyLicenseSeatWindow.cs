namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy license seat window is in its life cycle.</summary>
public enum LegacyLicenseSeatWindowStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A legacy license seat window. Its code is unique within a tenant.</summary>
public sealed class LegacyLicenseSeatWindow(string code, string title, double ceiling, string jobCode, int graceDays, int priority, bool active, double threshold, string name, LegacyLicenseSeatWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string JobCode { get; set; } = jobCode;
    public int GraceDays { get; set; } = graceDays;
    public int Priority { get; set; } = priority;
    public bool Active { get; set; } = active;
    public double Threshold { get; set; } = threshold;
    public string Name { get; set; } = name;
    public LegacyLicenseSeatWindowStatus Status { get; set; } = status;

    /// <summary>Starts a legacy license seat window in status idle.</summary>
    public static LegacyLicenseSeatWindow NewLegacyLicenseSeatWindow(string code, string title)
    {
        return new LegacyLicenseSeatWindow(
            code,
            title,
            0.0,
            "",
            0,
            0,
            false,
            0.0,
            "",
            LegacyLicenseSeatWindowStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyLicenseSeatWindowStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyLicenseSeatWindowStatus next)
    {
        switch (Status)
        {
            case LegacyLicenseSeatWindowStatus.Idle:
                switch (next)
                {
                    case LegacyLicenseSeatWindowStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyLicenseSeatWindowStatus.Warming:
                switch (next)
                {
                    case LegacyLicenseSeatWindowStatus.Ready:
                    case LegacyLicenseSeatWindowStatus.Stopped:
                        return true;
                }
                return false;
            case LegacyLicenseSeatWindowStatus.Ready:
                return next == LegacyLicenseSeatWindowStatus.Draining || next == LegacyLicenseSeatWindowStatus.Stopped;
            case LegacyLicenseSeatWindowStatus.Draining:
                switch (next)
                {
                    case LegacyLicenseSeatWindowStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyLicenseSeatWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy license seat window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Threshold - (double)Priority, 2);
    }

    public LegacyLicenseSeatWindow Copy()
    {
        return new LegacyLicenseSeatWindow(
            Code,
            Title,
            Ceiling,
            JobCode,
            GraceDays,
            Priority,
            Active,
            Threshold,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
