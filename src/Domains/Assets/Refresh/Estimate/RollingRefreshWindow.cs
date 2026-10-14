namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling refresh window is in its life cycle.</summary>
public enum RollingRefreshWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A rolling refresh window. Its code is unique within a tenant.</summary>
public sealed class RollingRefreshWindow(string code, string label, string phone, string region, double weight, int sequence, int windowDays, RollingRefreshWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public string Region { get; set; } = region;
    public double Weight { get; set; } = weight;
    public int Sequence { get; set; } = sequence;
    public int WindowDays { get; set; } = windowDays;
    public RollingRefreshWindowStatus Status { get; set; } = status;

    /// <summary>Starts a rolling refresh window in status draft.</summary>
    public static RollingRefreshWindow NewRollingRefreshWindow(string code, string label)
    {
        return new RollingRefreshWindow(code, label, "", "", 0.0, 0, 0, RollingRefreshWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRefreshWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRefreshWindowStatus next)
    {
        switch (Status)
        {
            case RollingRefreshWindowStatus.Draft:
                return next == RollingRefreshWindowStatus.Pending;
            case RollingRefreshWindowStatus.Pending:
                return next == RollingRefreshWindowStatus.Suspended;
            case RollingRefreshWindowStatus.Active:
                return next == RollingRefreshWindowStatus.Suspended;
            case RollingRefreshWindowStatus.Suspended:
                return next == RollingRefreshWindowStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingRefreshWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling refresh window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Weight + (double)Sequence, 2);
    }

    public RollingRefreshWindow Copy()
    {
        return new RollingRefreshWindow(Code, Label, Phone, Region, Weight, Sequence, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
