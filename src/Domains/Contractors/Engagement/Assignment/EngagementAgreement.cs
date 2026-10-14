namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement agreement is in its life cycle.</summary>
public enum EngagementAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A engagement agreement as the engagement team keeps it.</summary>
public sealed class EngagementAgreement(string code, string label, string managerId, string title, int durationMinutes, int batchSize, EngagementAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int BatchSize { get; set; } = batchSize;
    public EngagementAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a engagement agreement in status new.</summary>
    public static EngagementAgreement NewEngagementAgreement(string code, string label)
    {
        return new EngagementAgreement(code, label, "", "", 0, 0, EngagementAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementAgreementStatus next)
    {
        switch (Status)
        {
            case EngagementAgreementStatus.New:
                return next == EngagementAgreementStatus.Running || next == EngagementAgreementStatus.Failed;
            case EngagementAgreementStatus.Queued:
                return next == EngagementAgreementStatus.Running || next == EngagementAgreementStatus.Done;
            case EngagementAgreementStatus.Running:
                return next == EngagementAgreementStatus.Done || next == EngagementAgreementStatus.Failed;
            case EngagementAgreementStatus.Done:
                switch (next)
                {
                    case EngagementAgreementStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)BatchSize + (double)DurationMinutes, 2);
    }

    public EngagementAgreement Copy()
    {
        return new EngagementAgreement(Code, Label, ManagerId, Title, DurationMinutes, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
