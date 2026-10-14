namespace Acme.Workforce.Workplace.Visitor.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor schedule is in its life cycle.</summary>
public enum VisitorScheduleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a visitor schedule and its current status.</summary>
public sealed class VisitorSchedule(string code, string label, string externalRef, string salaryBand, string segment, int endsAt, string jobCode, int createdAt, double weight, VisitorScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string SalaryBand { get; set; } = salaryBand;
    public string Segment { get; set; } = segment;
    public int EndsAt { get; set; } = endsAt;
    public string JobCode { get; set; } = jobCode;
    public int CreatedAt { get; set; } = createdAt;
    public double Weight { get; set; } = weight;
    public VisitorScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a visitor schedule in status requested.</summary>
    public static VisitorSchedule NewVisitorSchedule(string code, string label)
    {
        return new VisitorSchedule(code, label, "", "", "", 0, "", 0, 0.0, VisitorScheduleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorScheduleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorScheduleStatus next)
    {
        switch (Status)
        {
            case VisitorScheduleStatus.Requested:
                return next == VisitorScheduleStatus.Granted;
            case VisitorScheduleStatus.Granted:
                return next == VisitorScheduleStatus.Revoked || next == VisitorScheduleStatus.Expired;
            case VisitorScheduleStatus.Revoked:
                switch (next)
                {
                    case VisitorScheduleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)EndsAt * Weight, 2);
    }

    public VisitorSchedule Copy()
    {
        return new VisitorSchedule(
            Code,
            Label,
            ExternalRef,
            SalaryBand,
            Segment,
            EndsAt,
            JobCode,
            CreatedAt,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
