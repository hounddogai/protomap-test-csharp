namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual reorg profile is in its life cycle.</summary>
public enum ManualReorgProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a manual reorg profile and its current status.</summary>
public sealed class ManualReorgProfile(string code, string name, double fte, string grade, int attempts, ManualReorgProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public string Grade { get; set; } = grade;
    public int Attempts { get; set; } = attempts;
    public ManualReorgProfileStatus Status { get; set; } = status;

    /// <summary>Starts a manual reorg profile in status proposed.</summary>
    public static ManualReorgProfile NewManualReorgProfile(string code, string name)
    {
        return new ManualReorgProfile(code, name, 0.0, "", 0, ManualReorgProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualReorgProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualReorgProfileStatus next)
    {
        switch (Status)
        {
            case ManualReorgProfileStatus.Proposed:
                switch (next)
                {
                    case ManualReorgProfileStatus.Approved:
                        return true;
                }
                return false;
            case ManualReorgProfileStatus.Approved:
                return next == ManualReorgProfileStatus.Rejected;
            case ManualReorgProfileStatus.Rejected:
                switch (next)
                {
                    case ManualReorgProfileStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualReorgProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual reorg profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual reorg profile"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Fte + (double)Attempts, 2);
    }

    public ManualReorgProfile Copy()
    {
        return new ManualReorgProfile(Code, Name, Fte, Grade, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
