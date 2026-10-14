namespace Acme.Workforce.Talent.Nomination.NominationWindow;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped nomination window is in its life cycle.</summary>
public enum CappedNominationWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One capped nomination window, with what the planners need to act on it.</summary>
public sealed class CappedNominationWindow(string code, string label, double hours, double amount, string name, CappedNominationWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public double Amount { get; set; } = amount;
    public string Name { get; set; } = name;
    public CappedNominationWindowStatus Status { get; set; } = status;

    /// <summary>Starts a capped nomination window in status proposed.</summary>
    public static CappedNominationWindow NewCappedNominationWindow(string code, string label)
    {
        return new CappedNominationWindow(code, label, 0.0, 0.0, "", CappedNominationWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedNominationWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedNominationWindowStatus next)
    {
        switch (Status)
        {
            case CappedNominationWindowStatus.Proposed:
                return next == CappedNominationWindowStatus.Rejected || next == CappedNominationWindowStatus.Withdrawn;
            case CappedNominationWindowStatus.Approved:
                return next == CappedNominationWindowStatus.Rejected || next == CappedNominationWindowStatus.Withdrawn;
            case CappedNominationWindowStatus.Rejected:
                switch (next)
                {
                    case CappedNominationWindowStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedNominationWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped nomination window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetExposure()
    {
        return Math.Round(Amount + Hours, 2);
    }

    public CappedNominationWindow Copy()
    {
        return new CappedNominationWindow(Code, Label, Hours, Amount, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
