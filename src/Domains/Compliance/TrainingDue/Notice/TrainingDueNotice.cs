namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due notice is in its life cycle.</summary>
public enum TrainingDueNoticeStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A training due notice. Its code is unique within a tenant.</summary>
public sealed class TrainingDueNotice(string code, string label, string salaryBand, int attempts, int graceDays, string phone, string summary, double threshold, string costCenter, TrainingDueNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public int Attempts { get; set; } = attempts;
    public int GraceDays { get; set; } = graceDays;
    public string Phone { get; set; } = phone;
    public string Summary { get; set; } = summary;
    public double Threshold { get; set; } = threshold;
    public string CostCenter { get; set; } = costCenter;
    public TrainingDueNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a training due notice in status planned.</summary>
    public static TrainingDueNotice NewTrainingDueNotice(string code, string label)
    {
        return new TrainingDueNotice(code, label, "", 0, 0, "", "", 0.0, "", TrainingDueNoticeStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueNoticeStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueNoticeStatus next)
    {
        switch (Status)
        {
            case TrainingDueNoticeStatus.Planned:
                switch (next)
                {
                    case TrainingDueNoticeStatus.Scheduled:
                        return true;
                }
                return false;
            case TrainingDueNoticeStatus.Scheduled:
                switch (next)
                {
                    case TrainingDueNoticeStatus.Completed:
                        return true;
                }
                return false;
            case TrainingDueNoticeStatus.InProgress:
                return next == TrainingDueNoticeStatus.Completed || next == TrainingDueNoticeStatus.Cancelled;
            case TrainingDueNoticeStatus.Completed:
                switch (next)
                {
                    case TrainingDueNoticeStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)GraceDays * (double)Attempts, 2);
    }

    public TrainingDueNotice Copy()
    {
        return new TrainingDueNotice(
            Code,
            Label,
            SalaryBand,
            Attempts,
            GraceDays,
            Phone,
            Summary,
            Threshold,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
