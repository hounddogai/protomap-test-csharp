namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted visitor entry is in its life cycle.</summary>
public enum WeightedVisitorEntryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A weighted visitor entry. Its code is unique within a tenant.</summary>
public sealed class WeightedVisitorEntry(string code, string name, string badge, string phone, string locale, string timezone, int windowDays, bool approved, WeightedVisitorEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public string Phone { get; set; } = phone;
    public string Locale { get; set; } = locale;
    public string Timezone { get; set; } = timezone;
    public int WindowDays { get; set; } = windowDays;
    public bool Approved { get; set; } = approved;
    public WeightedVisitorEntryStatus Status { get; set; } = status;

    /// <summary>Starts a weighted visitor entry in status requested.</summary>
    public static WeightedVisitorEntry NewWeightedVisitorEntry(string code, string name)
    {
        return new WeightedVisitorEntry(code, name, "", "", "", "", 0, false, WeightedVisitorEntryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedVisitorEntryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedVisitorEntryStatus next)
    {
        switch (Status)
        {
            case WeightedVisitorEntryStatus.Requested:
                return next == WeightedVisitorEntryStatus.Granted;
            case WeightedVisitorEntryStatus.Granted:
                switch (next)
                {
                    case WeightedVisitorEntryStatus.Expired:
                        return true;
                }
                return false;
            case WeightedVisitorEntryStatus.Revoked:
                switch (next)
                {
                    case WeightedVisitorEntryStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedVisitorEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted visitor entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public WeightedVisitorEntry Copy()
    {
        return new WeightedVisitorEntry(Code, Name, Badge, Phone, Locale, Timezone, WindowDays, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
