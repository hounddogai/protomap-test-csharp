namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle agreement is in its life cycle.</summary>
public enum MeritCycleAgreementStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One merit cycle agreement, with what the planners need to act on it.</summary>
public sealed class MeritCycleAgreement(string code, string name, string description, double floorValue, string jobCode, MeritCycleAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public double FloorValue { get; set; } = floorValue;
    public string JobCode { get; set; } = jobCode;
    public MeritCycleAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle agreement in status requested.</summary>
    public static MeritCycleAgreement NewMeritCycleAgreement(string code, string name)
    {
        return new MeritCycleAgreement(code, name, "", 0.0, "", MeritCycleAgreementStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleAgreementStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleAgreementStatus next)
    {
        switch (Status)
        {
            case MeritCycleAgreementStatus.Requested:
                switch (next)
                {
                    case MeritCycleAgreementStatus.Expired:
                        return true;
                }
                return false;
            case MeritCycleAgreementStatus.Granted:
                switch (next)
                {
                    case MeritCycleAgreementStatus.Revoked:
                    case MeritCycleAgreementStatus.Expired:
                        return true;
                }
                return false;
            case MeritCycleAgreementStatus.Revoked:
                return next == MeritCycleAgreementStatus.Expired;
        }
        return false;
    }

    public void MoveTo(MeritCycleAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public MeritCycleAgreement Copy()
    {
        return new MeritCycleAgreement(Code, Name, Description, FloorValue, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
