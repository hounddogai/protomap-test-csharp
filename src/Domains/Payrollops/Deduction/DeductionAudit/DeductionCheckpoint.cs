namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction checkpoint is in its life cycle.</summary>
public enum DeductionCheckpointStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One deduction checkpoint, with what the planners need to act on it.</summary>
public sealed class DeductionCheckpoint(string code, string name, string country, double cost, int sequence, bool approved, string title, string grade, int updatedAt, string jobCode, DeductionCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double Cost { get; set; } = cost;
    public int Sequence { get; set; } = sequence;
    public bool Approved { get; set; } = approved;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public int UpdatedAt { get; set; } = updatedAt;
    public string JobCode { get; set; } = jobCode;
    public DeductionCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a deduction checkpoint in status created.</summary>
    public static DeductionCheckpoint NewDeductionCheckpoint(string code, string name)
    {
        return new DeductionCheckpoint(
            code,
            name,
            "",
            0.0,
            0,
            false,
            "",
            "",
            0,
            "",
            DeductionCheckpointStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionCheckpointStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionCheckpointStatus next)
    {
        switch (Status)
        {
            case DeductionCheckpointStatus.Created:
                switch (next)
                {
                    case DeductionCheckpointStatus.Published:
                    case DeductionCheckpointStatus.Retired:
                        return true;
                }
                return false;
            case DeductionCheckpointStatus.Validated:
                return next == DeductionCheckpointStatus.Published || next == DeductionCheckpointStatus.Retired;
            case DeductionCheckpointStatus.Published:
                switch (next)
                {
                    case DeductionCheckpointStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Sequence * Cost, 2);
    }

    public DeductionCheckpoint Copy()
    {
        return new DeductionCheckpoint(
            Code,
            Name,
            Country,
            Cost,
            Sequence,
            Approved,
            Title,
            Grade,
            UpdatedAt,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
