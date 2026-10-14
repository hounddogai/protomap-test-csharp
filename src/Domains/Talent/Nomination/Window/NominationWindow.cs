namespace Acme.Workforce.Talent.Nomination.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination window is in its life cycle.</summary>
public enum NominationWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A nomination window. Its code is unique within a tenant.</summary>
public sealed class NominationWindow(string code, string label, string country, double ratio, int revision, NominationWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public double Ratio { get; set; } = ratio;
    public int Revision { get; set; } = revision;
    public NominationWindowStatus Status { get; set; } = status;

    /// <summary>Starts a nomination window in status proposed.</summary>
    public static NominationWindow NewNominationWindow(string code, string label)
    {
        return new NominationWindow(code, label, "", 0.0, 0, NominationWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationWindowStatus next)
    {
        switch (Status)
        {
            case NominationWindowStatus.Proposed:
                return next == NominationWindowStatus.Rejected;
            case NominationWindowStatus.Approved:
                switch (next)
                {
                    case NominationWindowStatus.Withdrawn:
                        return true;
                }
                return false;
            case NominationWindowStatus.Rejected:
                return next == NominationWindowStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(NominationWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ratio + (double)Revision, 2);
    }

    public NominationWindow Copy()
    {
        return new NominationWindow(Code, Label, Country, Ratio, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
