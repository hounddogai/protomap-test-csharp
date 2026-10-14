namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered career path override is in its life cycle.</summary>
public enum TieredCareerPathOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tiered career path override as the career path team keeps it.</summary>
public sealed class TieredCareerPathOverride(string code, string label, string jobCode, int expiresAt, string location, bool active, bool pinned, TieredCareerPathOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Location { get; set; } = location;
    public bool Active { get; set; } = active;
    public bool Pinned { get; set; } = pinned;
    public TieredCareerPathOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a tiered career path override in status planned.</summary>
    public static TieredCareerPathOverride NewTieredCareerPathOverride(string code, string label)
    {
        return new TieredCareerPathOverride(
            code,
            label,
            "",
            0,
            "",
            false,
            false,
            TieredCareerPathOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredCareerPathOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredCareerPathOverrideStatus next)
    {
        switch (Status)
        {
            case TieredCareerPathOverrideStatus.Planned:
                return next == TieredCareerPathOverrideStatus.InProgress || next == TieredCareerPathOverrideStatus.Cancelled;
            case TieredCareerPathOverrideStatus.Scheduled:
                switch (next)
                {
                    case TieredCareerPathOverrideStatus.InProgress:
                    case TieredCareerPathOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredCareerPathOverrideStatus.InProgress:
                return next == TieredCareerPathOverrideStatus.Completed || next == TieredCareerPathOverrideStatus.Cancelled;
            case TieredCareerPathOverrideStatus.Completed:
                return next == TieredCareerPathOverrideStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TieredCareerPathOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered career path override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public TieredCareerPathOverride Copy()
    {
        return new TieredCareerPathOverride(Code, Label, JobCode, ExpiresAt, Location, Active, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
