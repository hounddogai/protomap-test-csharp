namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual training due notice is in its life cycle.</summary>
public enum ManualTrainingDueNoticeStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A manual training due notice as the training due team keeps it.</summary>
public sealed class ManualTrainingDueNotice(string code, string name, int capacity, double margin, string employeeId, ManualTrainingDueNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public double Margin { get; set; } = margin;
    public string EmployeeId { get; set; } = employeeId;
    public ManualTrainingDueNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a manual training due notice in status requested.</summary>
    public static ManualTrainingDueNotice NewManualTrainingDueNotice(string code, string name)
    {
        return new ManualTrainingDueNotice(code, name, 0, 0.0, "", ManualTrainingDueNoticeStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualTrainingDueNoticeStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualTrainingDueNoticeStatus next)
    {
        switch (Status)
        {
            case ManualTrainingDueNoticeStatus.Requested:
                return next == ManualTrainingDueNoticeStatus.Revoked || next == ManualTrainingDueNoticeStatus.Expired;
            case ManualTrainingDueNoticeStatus.Granted:
                switch (next)
                {
                    case ManualTrainingDueNoticeStatus.Revoked:
                    case ManualTrainingDueNoticeStatus.Expired:
                        return true;
                }
                return false;
            case ManualTrainingDueNoticeStatus.Revoked:
                return next == ManualTrainingDueNoticeStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ManualTrainingDueNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual training due notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedValue()
    {
        return Math.Round(Margin - (double)Capacity, 2);
    }

    public ManualTrainingDueNotice Copy()
    {
        return new ManualTrainingDueNotice(Code, Name, Capacity, Margin, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
