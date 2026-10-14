namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due exception is in its life cycle.</summary>
public enum TrainingDueExceptionStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A training due exception. Its code is unique within a tenant.</summary>
public sealed class TrainingDueException(string code, string title, int retries, int attempts, string department, string costCenter, TrainingDueExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public int Attempts { get; set; } = attempts;
    public string Department { get; set; } = department;
    public string CostCenter { get; set; } = costCenter;
    public TrainingDueExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a training due exception in status created.</summary>
    public static TrainingDueException NewTrainingDueException(string code, string title)
    {
        return new TrainingDueException(code, title, 0, 0, "", "", TrainingDueExceptionStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueExceptionStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueExceptionStatus next)
    {
        switch (Status)
        {
            case TrainingDueExceptionStatus.Created:
                switch (next)
                {
                    case TrainingDueExceptionStatus.Published:
                        return true;
                }
                return false;
            case TrainingDueExceptionStatus.Validated:
                return next == TrainingDueExceptionStatus.Published || next == TrainingDueExceptionStatus.Retired;
            case TrainingDueExceptionStatus.Published:
                switch (next)
                {
                    case TrainingDueExceptionStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Retries + (double)Attempts, 2);
    }

    public TrainingDueException Copy()
    {
        return new TrainingDueException(Code, Title, Retries, Attempts, Department, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
