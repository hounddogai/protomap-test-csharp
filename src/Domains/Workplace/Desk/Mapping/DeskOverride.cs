namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk override is in its life cycle.</summary>
public enum DeskOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a desk override and its current status.</summary>
public sealed class DeskOverride(string code, string label, int version, string employeeId, int startsAt, string externalRef, DeskOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string EmployeeId { get; set; } = employeeId;
    public int StartsAt { get; set; } = startsAt;
    public string ExternalRef { get; set; } = externalRef;
    public DeskOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a desk override in status open.</summary>
    public static DeskOverride NewDeskOverride(string code, string label)
    {
        return new DeskOverride(code, label, 0, "", 0, "", DeskOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskOverrideStatus next)
    {
        switch (Status)
        {
            case DeskOverrideStatus.Open:
                return next == DeskOverrideStatus.Resolved;
            case DeskOverrideStatus.InReview:
                switch (next)
                {
                    case DeskOverrideStatus.Resolved:
                        return true;
                }
                return false;
            case DeskOverrideStatus.Resolved:
                return next == DeskOverrideStatus.Reopened || next == DeskOverrideStatus.Archived;
            case DeskOverrideStatus.Reopened:
                return next == DeskOverrideStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DeskOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"desk override"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Version * (double)StartsAt, 2);
    }

    public DeskOverride Copy()
    {
        return new DeskOverride(Code, Label, Version, EmployeeId, StartsAt, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
