namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional training due notice is in its life cycle.</summary>
public enum RegionalTrainingDueNoticeStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A regional training due notice as the training due team keeps it.</summary>
public sealed class RegionalTrainingDueNotice(string code, string label, string externalRef, bool pinned, string description, string department, string costCenter, double score, RegionalTrainingDueNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public bool Pinned { get; set; } = pinned;
    public string Description { get; set; } = description;
    public string Department { get; set; } = department;
    public string CostCenter { get; set; } = costCenter;
    public double Score { get; set; } = score;
    public RegionalTrainingDueNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a regional training due notice in status requested.</summary>
    public static RegionalTrainingDueNotice NewRegionalTrainingDueNotice(string code, string label)
    {
        return new RegionalTrainingDueNotice(
            code,
            label,
            "",
            false,
            "",
            "",
            "",
            0.0,
            RegionalTrainingDueNoticeStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTrainingDueNoticeStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTrainingDueNoticeStatus next)
    {
        switch (Status)
        {
            case RegionalTrainingDueNoticeStatus.Requested:
                return next == RegionalTrainingDueNoticeStatus.Granted || next == RegionalTrainingDueNoticeStatus.Expired;
            case RegionalTrainingDueNoticeStatus.Granted:
                switch (next)
                {
                    case RegionalTrainingDueNoticeStatus.Expired:
                        return true;
                }
                return false;
            case RegionalTrainingDueNoticeStatus.Revoked:
                return next == RegionalTrainingDueNoticeStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RegionalTrainingDueNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional training due notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public RegionalTrainingDueNotice Copy()
    {
        return new RegionalTrainingDueNotice(
            Code,
            Label,
            ExternalRef,
            Pinned,
            Description,
            Department,
            CostCenter,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
