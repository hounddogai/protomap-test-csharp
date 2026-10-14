namespace Acme.Workforce.Contractors.Rate.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate preference is in its life cycle.</summary>
public enum RatePreferenceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One rate preference, with what the planners need to act on it.</summary>
public sealed class RatePreference(string code, string name, string source, bool approved, string department, double threshold, bool pinned, int graceDays, RatePreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public bool Approved { get; set; } = approved;
    public string Department { get; set; } = department;
    public double Threshold { get; set; } = threshold;
    public bool Pinned { get; set; } = pinned;
    public int GraceDays { get; set; } = graceDays;
    public RatePreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a rate preference in status requested.</summary>
    public static RatePreference NewRatePreference(string code, string name)
    {
        return new RatePreference(code, name, "", false, "", 0.0, false, 0, RatePreferenceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RatePreferenceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RatePreferenceStatus next)
    {
        switch (Status)
        {
            case RatePreferenceStatus.Requested:
                switch (next)
                {
                    case RatePreferenceStatus.Granted:
                    case RatePreferenceStatus.Expired:
                        return true;
                }
                return false;
            case RatePreferenceStatus.Granted:
                switch (next)
                {
                    case RatePreferenceStatus.Expired:
                        return true;
                }
                return false;
            case RatePreferenceStatus.Revoked:
                switch (next)
                {
                    case RatePreferenceStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RatePreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(Threshold - (double)GraceDays, 2);
    }

    public RatePreference Copy()
    {
        return new RatePreference(Code, Name, Source, Approved, Department, Threshold, Pinned, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
