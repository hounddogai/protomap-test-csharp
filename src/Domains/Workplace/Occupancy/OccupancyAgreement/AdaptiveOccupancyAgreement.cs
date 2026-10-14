namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive occupancy agreement is in its life cycle.</summary>
public enum AdaptiveOccupancyAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A adaptive occupancy agreement. Its code is unique within a tenant.</summary>
public sealed class AdaptiveOccupancyAgreement(string code, string name, string externalRef, string department, double discount, AdaptiveOccupancyAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public string Department { get; set; } = department;
    public double Discount { get; set; } = discount;
    public AdaptiveOccupancyAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive occupancy agreement in status new.</summary>
    public static AdaptiveOccupancyAgreement NewAdaptiveOccupancyAgreement(string code, string name)
    {
        return new AdaptiveOccupancyAgreement(code, name, "", "", 0.0, AdaptiveOccupancyAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveOccupancyAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveOccupancyAgreementStatus next)
    {
        switch (Status)
        {
            case AdaptiveOccupancyAgreementStatus.New:
                return next == AdaptiveOccupancyAgreementStatus.Queued || next == AdaptiveOccupancyAgreementStatus.Done;
            case AdaptiveOccupancyAgreementStatus.Queued:
                return next == AdaptiveOccupancyAgreementStatus.Failed;
            case AdaptiveOccupancyAgreementStatus.Running:
                return next == AdaptiveOccupancyAgreementStatus.Done || next == AdaptiveOccupancyAgreementStatus.Failed;
            case AdaptiveOccupancyAgreementStatus.Done:
                return next == AdaptiveOccupancyAgreementStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveOccupancyAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive occupancy agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public AdaptiveOccupancyAgreement Copy()
    {
        return new AdaptiveOccupancyAgreement(Code, Name, ExternalRef, Department, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
