namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge preference is in its life cycle.</summary>
public enum AccessBadgePreferenceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A access badge preference. Its code is unique within a tenant.</summary>
public sealed class AccessBadgePreference(string code, string title, string summary, double floorValue, double ceiling, double threshold, AccessBadgePreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public double FloorValue { get; set; } = floorValue;
    public double Ceiling { get; set; } = ceiling;
    public double Threshold { get; set; } = threshold;
    public AccessBadgePreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a access badge preference in status draft.</summary>
    public static AccessBadgePreference NewAccessBadgePreference(string code, string title)
    {
        return new AccessBadgePreference(code, title, "", 0.0, 0.0, 0.0, AccessBadgePreferenceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgePreferenceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgePreferenceStatus next)
    {
        switch (Status)
        {
            case AccessBadgePreferenceStatus.Draft:
                switch (next)
                {
                    case AccessBadgePreferenceStatus.Suspended:
                    case AccessBadgePreferenceStatus.Closed:
                        return true;
                }
                return false;
            case AccessBadgePreferenceStatus.Pending:
                switch (next)
                {
                    case AccessBadgePreferenceStatus.Suspended:
                    case AccessBadgePreferenceStatus.Closed:
                        return true;
                }
                return false;
            case AccessBadgePreferenceStatus.Active:
                switch (next)
                {
                    case AccessBadgePreferenceStatus.Suspended:
                        return true;
                }
                return false;
            case AccessBadgePreferenceStatus.Suspended:
                return next == AccessBadgePreferenceStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AccessBadgePreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Threshold - Ceiling, 2);
    }

    public AccessBadgePreference Copy()
    {
        return new AccessBadgePreference(Code, Title, Summary, FloorValue, Ceiling, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
