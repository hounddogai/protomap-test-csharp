namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling training due exception is in its life cycle.</summary>
public enum RollingTrainingDueExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a rolling training due exception and its current status.</summary>
public sealed class RollingTrainingDueException(string code, string name, int sequence, string ownerId, int createdAt, string timezone, double margin, RollingTrainingDueExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public string OwnerId { get; set; } = ownerId;
    public int CreatedAt { get; set; } = createdAt;
    public string Timezone { get; set; } = timezone;
    public double Margin { get; set; } = margin;
    public RollingTrainingDueExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a rolling training due exception in status proposed.</summary>
    public static RollingTrainingDueException NewRollingTrainingDueException(string code, string name)
    {
        return new RollingTrainingDueException(
            code,
            name,
            0,
            "",
            0,
            "",
            0.0,
            RollingTrainingDueExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingTrainingDueExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingTrainingDueExceptionStatus next)
    {
        switch (Status)
        {
            case RollingTrainingDueExceptionStatus.Proposed:
                return next == RollingTrainingDueExceptionStatus.Approved || next == RollingTrainingDueExceptionStatus.Rejected;
            case RollingTrainingDueExceptionStatus.Approved:
                return next == RollingTrainingDueExceptionStatus.Withdrawn;
            case RollingTrainingDueExceptionStatus.Rejected:
                return next == RollingTrainingDueExceptionStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RollingTrainingDueExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling training due exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"rolling training due exception"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Sequence * Margin, 2);
    }

    public RollingTrainingDueException Copy()
    {
        return new RollingTrainingDueException(Code, Name, Sequence, OwnerId, CreatedAt, Timezone, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
