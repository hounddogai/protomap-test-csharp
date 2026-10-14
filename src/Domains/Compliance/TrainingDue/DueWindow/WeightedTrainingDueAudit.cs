namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted training due audit is in its life cycle.</summary>
public enum WeightedTrainingDueAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One weighted training due audit, with what the planners need to act on it.</summary>
public sealed class WeightedTrainingDueAudit(string code, string title, int sequence, string segment, string costCenter, string timezone, double hours, string unit, WeightedTrainingDueAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public string Segment { get; set; } = segment;
    public string CostCenter { get; set; } = costCenter;
    public string Timezone { get; set; } = timezone;
    public double Hours { get; set; } = hours;
    public string Unit { get; set; } = unit;
    public WeightedTrainingDueAuditStatus Status { get; set; } = status;

    /// <summary>Starts a weighted training due audit in status created.</summary>
    public static WeightedTrainingDueAudit NewWeightedTrainingDueAudit(string code, string title)
    {
        return new WeightedTrainingDueAudit(
            code,
            title,
            0,
            "",
            "",
            "",
            0.0,
            "",
            WeightedTrainingDueAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedTrainingDueAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedTrainingDueAuditStatus next)
    {
        switch (Status)
        {
            case WeightedTrainingDueAuditStatus.Created:
                switch (next)
                {
                    case WeightedTrainingDueAuditStatus.Retired:
                        return true;
                }
                return false;
            case WeightedTrainingDueAuditStatus.Validated:
                return next == WeightedTrainingDueAuditStatus.Published || next == WeightedTrainingDueAuditStatus.Retired;
            case WeightedTrainingDueAuditStatus.Published:
                return next == WeightedTrainingDueAuditStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedTrainingDueAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted training due audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted training due audit"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Sequence + Hours, 2);
    }

    public WeightedTrainingDueAudit Copy()
    {
        return new WeightedTrainingDueAudit(
            Code,
            Title,
            Sequence,
            Segment,
            CostCenter,
            Timezone,
            Hours,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
