namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled training due draft is in its life cycle.</summary>
public enum ScheduledTrainingDueDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A scheduled training due draft as the training due team keeps it.</summary>
public sealed class ScheduledTrainingDueDraft(string code, string name, string reviewer, string managerId, bool active, string externalRef, string badge, int capacity, int limit, ScheduledTrainingDueDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public string ManagerId { get; set; } = managerId;
    public bool Active { get; set; } = active;
    public string ExternalRef { get; set; } = externalRef;
    public string Badge { get; set; } = badge;
    public int Capacity { get; set; } = capacity;
    public int Limit { get; set; } = limit;
    public ScheduledTrainingDueDraftStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled training due draft in status requested.</summary>
    public static ScheduledTrainingDueDraft NewScheduledTrainingDueDraft(string code, string name)
    {
        return new ScheduledTrainingDueDraft(
            code,
            name,
            "",
            "",
            false,
            "",
            "",
            0,
            0,
            ScheduledTrainingDueDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledTrainingDueDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledTrainingDueDraftStatus next)
    {
        switch (Status)
        {
            case ScheduledTrainingDueDraftStatus.Requested:
                return next == ScheduledTrainingDueDraftStatus.Expired;
            case ScheduledTrainingDueDraftStatus.Granted:
                return next == ScheduledTrainingDueDraftStatus.Revoked || next == ScheduledTrainingDueDraftStatus.Expired;
            case ScheduledTrainingDueDraftStatus.Revoked:
                return next == ScheduledTrainingDueDraftStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ScheduledTrainingDueDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled training due draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Limit + (double)Capacity, 2);
    }

    public ScheduledTrainingDueDraft Copy()
    {
        return new ScheduledTrainingDueDraft(
            Code,
            Name,
            Reviewer,
            ManagerId,
            Active,
            ExternalRef,
            Badge,
            Capacity,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
