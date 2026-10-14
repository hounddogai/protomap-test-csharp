namespace Acme.Workforce.Compliance.Exception.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception tier is in its life cycle.</summary>
public enum ExceptionTierStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a exception tier and its current status.</summary>
public sealed class ExceptionTier(string code, string name, string notes, double maximum, string region, int graceDays, string label, int priority, string team, string phone, bool archived, ExceptionTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public double Maximum { get; set; } = maximum;
    public string Region { get; set; } = region;
    public int GraceDays { get; set; } = graceDays;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Team { get; set; } = team;
    public string Phone { get; set; } = phone;
    public bool Archived { get; set; } = archived;
    public ExceptionTierStatus Status { get; set; } = status;

    /// <summary>Starts a exception tier in status requested.</summary>
    public static ExceptionTier NewExceptionTier(string code, string name)
    {
        return new ExceptionTier(code, name, "", 0.0, "", 0, "", 0, "", "", false, ExceptionTierStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionTierStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionTierStatus next)
    {
        switch (Status)
        {
            case ExceptionTierStatus.Requested:
                switch (next)
                {
                    case ExceptionTierStatus.Revoked:
                        return true;
                }
                return false;
            case ExceptionTierStatus.Granted:
                return next == ExceptionTierStatus.Expired;
            case ExceptionTierStatus.Revoked:
                return next == ExceptionTierStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ExceptionTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Maximum - (double)Priority, 2);
    }

    public ExceptionTier Copy()
    {
        return new ExceptionTier(
            Code,
            Name,
            Notes,
            Maximum,
            Region,
            GraceDays,
            Label,
            Priority,
            Team,
            Phone,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
