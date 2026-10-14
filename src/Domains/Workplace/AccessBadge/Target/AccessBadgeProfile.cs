namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge profile is in its life cycle.</summary>
public enum AccessBadgeProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a access badge profile and its current status.</summary>
public sealed class AccessBadgeProfile(string code, string name, double cost, string grade, double ceiling, string notes, int graceDays, AccessBadgeProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public string Notes { get; set; } = notes;
    public int GraceDays { get; set; } = graceDays;
    public AccessBadgeProfileStatus Status { get; set; } = status;

    /// <summary>Starts a access badge profile in status draft.</summary>
    public static AccessBadgeProfile NewAccessBadgeProfile(string code, string name)
    {
        return new AccessBadgeProfile(code, name, 0.0, "", 0.0, "", 0, AccessBadgeProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeProfileStatus next)
    {
        switch (Status)
        {
            case AccessBadgeProfileStatus.Draft:
                return next == AccessBadgeProfileStatus.Pending || next == AccessBadgeProfileStatus.Closed;
            case AccessBadgeProfileStatus.Pending:
                return next == AccessBadgeProfileStatus.Closed;
            case AccessBadgeProfileStatus.Active:
                return next == AccessBadgeProfileStatus.Suspended || next == AccessBadgeProfileStatus.Closed;
            case AccessBadgeProfileStatus.Suspended:
                return next == AccessBadgeProfileStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AccessBadgeProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Ceiling - (double)GraceDays, 2);
    }

    public AccessBadgeProfile Copy()
    {
        return new AccessBadgeProfile(Code, Name, Cost, Grade, Ceiling, Notes, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
