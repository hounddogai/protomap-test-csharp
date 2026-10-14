namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict training due draft is in its life cycle.</summary>
public enum StrictTrainingDueDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A strict training due draft as the training due team keeps it.</summary>
public sealed class StrictTrainingDueDraft(string code, string title, double ceiling, int updatedAt, double rating, string externalRef, string employeeId, string channel, StrictTrainingDueDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Rating { get; set; } = rating;
    public string ExternalRef { get; set; } = externalRef;
    public string EmployeeId { get; set; } = employeeId;
    public string Channel { get; set; } = channel;
    public StrictTrainingDueDraftStatus Status { get; set; } = status;

    /// <summary>Starts a strict training due draft in status draft.</summary>
    public static StrictTrainingDueDraft NewStrictTrainingDueDraft(string code, string title)
    {
        return new StrictTrainingDueDraft(code, title, 0.0, 0, 0.0, "", "", "", StrictTrainingDueDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictTrainingDueDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictTrainingDueDraftStatus next)
    {
        switch (Status)
        {
            case StrictTrainingDueDraftStatus.Draft:
                return next == StrictTrainingDueDraftStatus.Pending;
            case StrictTrainingDueDraftStatus.Pending:
                return next == StrictTrainingDueDraftStatus.Suspended;
            case StrictTrainingDueDraftStatus.Active:
                return next == StrictTrainingDueDraftStatus.Suspended || next == StrictTrainingDueDraftStatus.Closed;
            case StrictTrainingDueDraftStatus.Suspended:
                return next == StrictTrainingDueDraftStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StrictTrainingDueDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict training due draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"strict training due draft"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Ceiling * (double)UpdatedAt, 2);
    }

    public StrictTrainingDueDraft Copy()
    {
        return new StrictTrainingDueDraft(
            Code,
            Title,
            Ceiling,
            UpdatedAt,
            Rating,
            ExternalRef,
            EmployeeId,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
