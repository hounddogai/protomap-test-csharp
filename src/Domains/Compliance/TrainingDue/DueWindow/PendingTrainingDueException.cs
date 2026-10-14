namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending training due exception is in its life cycle.</summary>
public enum PendingTrainingDueExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending training due exception. Its code is unique within a tenant.</summary>
public sealed class PendingTrainingDueException(string code, string label, string salaryBand, string country, int version, double rate, int createdAt, double ratio, int startsAt, string employeeId, PendingTrainingDueExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Country { get; set; } = country;
    public int Version { get; set; } = version;
    public double Rate { get; set; } = rate;
    public int CreatedAt { get; set; } = createdAt;
    public double Ratio { get; set; } = ratio;
    public int StartsAt { get; set; } = startsAt;
    public string EmployeeId { get; set; } = employeeId;
    public PendingTrainingDueExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a pending training due exception in status proposed.</summary>
    public static PendingTrainingDueException NewPendingTrainingDueException(string code, string label)
    {
        return new PendingTrainingDueException(
            code,
            label,
            "",
            "",
            0,
            0.0,
            0,
            0.0,
            0,
            "",
            PendingTrainingDueExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingTrainingDueExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingTrainingDueExceptionStatus next)
    {
        switch (Status)
        {
            case PendingTrainingDueExceptionStatus.Proposed:
                switch (next)
                {
                    case PendingTrainingDueExceptionStatus.Rejected:
                    case PendingTrainingDueExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
            case PendingTrainingDueExceptionStatus.Approved:
                return next == PendingTrainingDueExceptionStatus.Withdrawn;
            case PendingTrainingDueExceptionStatus.Rejected:
                switch (next)
                {
                    case PendingTrainingDueExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingTrainingDueExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending training due exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rate + (double)Version, 2);
    }

    public PendingTrainingDueException Copy()
    {
        return new PendingTrainingDueException(
            Code,
            Label,
            SalaryBand,
            Country,
            Version,
            Rate,
            CreatedAt,
            Ratio,
            StartsAt,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
