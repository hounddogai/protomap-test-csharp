namespace Acme.Workforce.Compliance.TrainingDue.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict training due quota is in its life cycle.</summary>
public enum StrictTrainingDueQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict training due quota as the training due team keeps it.</summary>
public sealed class StrictTrainingDueQuota(string code, string label, string summary, int revision, string team, bool pinned, StrictTrainingDueQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public int Revision { get; set; } = revision;
    public string Team { get; set; } = team;
    public bool Pinned { get; set; } = pinned;
    public StrictTrainingDueQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a strict training due quota in status proposed.</summary>
    public static StrictTrainingDueQuota NewStrictTrainingDueQuota(string code, string label)
    {
        return new StrictTrainingDueQuota(code, label, "", 0, "", false, StrictTrainingDueQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictTrainingDueQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictTrainingDueQuotaStatus next)
    {
        switch (Status)
        {
            case StrictTrainingDueQuotaStatus.Proposed:
                return next == StrictTrainingDueQuotaStatus.Rejected || next == StrictTrainingDueQuotaStatus.Withdrawn;
            case StrictTrainingDueQuotaStatus.Approved:
                return next == StrictTrainingDueQuotaStatus.Withdrawn;
            case StrictTrainingDueQuotaStatus.Rejected:
                switch (next)
                {
                    case StrictTrainingDueQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictTrainingDueQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict training due quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public StrictTrainingDueQuota Copy()
    {
        return new StrictTrainingDueQuota(Code, Label, Summary, Revision, Team, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
