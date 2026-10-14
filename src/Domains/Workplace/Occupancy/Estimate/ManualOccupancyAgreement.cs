namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual occupancy agreement is in its life cycle.</summary>
public enum ManualOccupancyAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a manual occupancy agreement and its current status.</summary>
public sealed class ManualOccupancyAgreement(string code, string title, int graceDays, double ceiling, double minimum, ManualOccupancyAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public double Ceiling { get; set; } = ceiling;
    public double Minimum { get; set; } = minimum;
    public ManualOccupancyAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a manual occupancy agreement in status new.</summary>
    public static ManualOccupancyAgreement NewManualOccupancyAgreement(string code, string title)
    {
        return new ManualOccupancyAgreement(code, title, 0, 0.0, 0.0, ManualOccupancyAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualOccupancyAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualOccupancyAgreementStatus next)
    {
        switch (Status)
        {
            case ManualOccupancyAgreementStatus.New:
                switch (next)
                {
                    case ManualOccupancyAgreementStatus.Failed:
                        return true;
                }
                return false;
            case ManualOccupancyAgreementStatus.Queued:
                return next == ManualOccupancyAgreementStatus.Done || next == ManualOccupancyAgreementStatus.Failed;
            case ManualOccupancyAgreementStatus.Running:
                return next == ManualOccupancyAgreementStatus.Done || next == ManualOccupancyAgreementStatus.Failed;
            case ManualOccupancyAgreementStatus.Done:
                return next == ManualOccupancyAgreementStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ManualOccupancyAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual occupancy agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual occupancy agreement"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Ceiling * Minimum, 2);
    }

    public ManualOccupancyAgreement Copy()
    {
        return new ManualOccupancyAgreement(Code, Title, GraceDays, Ceiling, Minimum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
