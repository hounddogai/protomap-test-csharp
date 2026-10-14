namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched training due notice is in its life cycle.</summary>
public enum BatchedTrainingDueNoticeStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One batched training due notice, with what the planners need to act on it.</summary>
public sealed class BatchedTrainingDueNotice(string code, string name, string salaryBand, double budget, string category, string summary, BatchedTrainingDueNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public double Budget { get; set; } = budget;
    public string Category { get; set; } = category;
    public string Summary { get; set; } = summary;
    public BatchedTrainingDueNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a batched training due notice in status idle.</summary>
    public static BatchedTrainingDueNotice NewBatchedTrainingDueNotice(string code, string name)
    {
        return new BatchedTrainingDueNotice(code, name, "", 0.0, "", "", BatchedTrainingDueNoticeStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTrainingDueNoticeStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTrainingDueNoticeStatus next)
    {
        switch (Status)
        {
            case BatchedTrainingDueNoticeStatus.Idle:
                switch (next)
                {
                    case BatchedTrainingDueNoticeStatus.Warming:
                    case BatchedTrainingDueNoticeStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedTrainingDueNoticeStatus.Warming:
                return next == BatchedTrainingDueNoticeStatus.Draining || next == BatchedTrainingDueNoticeStatus.Stopped;
            case BatchedTrainingDueNoticeStatus.Ready:
                switch (next)
                {
                    case BatchedTrainingDueNoticeStatus.Draining:
                    case BatchedTrainingDueNoticeStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedTrainingDueNoticeStatus.Draining:
                return next == BatchedTrainingDueNoticeStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BatchedTrainingDueNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched training due notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public BatchedTrainingDueNotice Copy()
    {
        return new BatchedTrainingDueNotice(Code, Name, SalaryBand, Budget, Category, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
