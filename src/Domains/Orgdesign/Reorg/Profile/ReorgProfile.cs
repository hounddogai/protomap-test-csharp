namespace Acme.Workforce.Orgdesign.Reorg.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg profile is in its life cycle.</summary>
public enum ReorgProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a reorg profile and its current status.</summary>
public sealed class ReorgProfile(string code, string label, int sequence, string location, double amount, ReorgProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public string Location { get; set; } = location;
    public double Amount { get; set; } = amount;
    public ReorgProfileStatus Status { get; set; } = status;

    /// <summary>Starts a reorg profile in status proposed.</summary>
    public static ReorgProfile NewReorgProfile(string code, string label)
    {
        return new ReorgProfile(code, label, 0, "", 0.0, ReorgProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgProfileStatus next)
    {
        switch (Status)
        {
            case ReorgProfileStatus.Proposed:
                return next == ReorgProfileStatus.Rejected || next == ReorgProfileStatus.Withdrawn;
            case ReorgProfileStatus.Approved:
                return next == ReorgProfileStatus.Rejected;
            case ReorgProfileStatus.Rejected:
                switch (next)
                {
                    case ReorgProfileStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Amount - (double)Sequence, 2);
    }

    public ReorgProfile Copy()
    {
        return new ReorgProfile(Code, Label, Sequence, Location, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
