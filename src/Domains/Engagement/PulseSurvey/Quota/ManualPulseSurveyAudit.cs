namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual pulse survey audit is in its life cycle.</summary>
public enum ManualPulseSurveyAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a manual pulse survey audit and its current status.</summary>
public sealed class ManualPulseSurveyAudit(string code, string label, string costCenter, int batchSize, string checksum, double rate, string reviewer, int leaveDays, int capacity, string badge, ManualPulseSurveyAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public int BatchSize { get; set; } = batchSize;
    public string Checksum { get; set; } = checksum;
    public double Rate { get; set; } = rate;
    public string Reviewer { get; set; } = reviewer;
    public int LeaveDays { get; set; } = leaveDays;
    public int Capacity { get; set; } = capacity;
    public string Badge { get; set; } = badge;
    public ManualPulseSurveyAuditStatus Status { get; set; } = status;

    /// <summary>Starts a manual pulse survey audit in status proposed.</summary>
    public static ManualPulseSurveyAudit NewManualPulseSurveyAudit(string code, string label)
    {
        return new ManualPulseSurveyAudit(
            code,
            label,
            "",
            0,
            "",
            0.0,
            "",
            0,
            0,
            "",
            ManualPulseSurveyAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPulseSurveyAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPulseSurveyAuditStatus next)
    {
        switch (Status)
        {
            case ManualPulseSurveyAuditStatus.Proposed:
                switch (next)
                {
                    case ManualPulseSurveyAuditStatus.Approved:
                        return true;
                }
                return false;
            case ManualPulseSurveyAuditStatus.Approved:
                switch (next)
                {
                    case ManualPulseSurveyAuditStatus.Withdrawn:
                        return true;
                }
                return false;
            case ManualPulseSurveyAuditStatus.Rejected:
                return next == ManualPulseSurveyAuditStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualPulseSurveyAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual pulse survey audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Capacity - Rate, 2);
    }

    public ManualPulseSurveyAudit Copy()
    {
        return new ManualPulseSurveyAudit(
            Code,
            Label,
            CostCenter,
            BatchSize,
            Checksum,
            Rate,
            Reviewer,
            LeaveDays,
            Capacity,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
