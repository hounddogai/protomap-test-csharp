namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation budget is in its life cycle.</summary>
public enum AttestationBudgetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A attestation budget. Its code is unique within a tenant.</summary>
public sealed class AttestationBudget(string code, string label, int leaveDays, string channel, bool archived, AttestationBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public string Channel { get; set; } = channel;
    public bool Archived { get; set; } = archived;
    public AttestationBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a attestation budget in status new.</summary>
    public static AttestationBudget NewAttestationBudget(string code, string label)
    {
        return new AttestationBudget(code, label, 0, "", false, AttestationBudgetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationBudgetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationBudgetStatus next)
    {
        switch (Status)
        {
            case AttestationBudgetStatus.New:
                switch (next)
                {
                    case AttestationBudgetStatus.Queued:
                    case AttestationBudgetStatus.Running:
                        return true;
                }
                return false;
            case AttestationBudgetStatus.Queued:
                return next == AttestationBudgetStatus.Running || next == AttestationBudgetStatus.Failed;
            case AttestationBudgetStatus.Running:
                return next == AttestationBudgetStatus.Done || next == AttestationBudgetStatus.Failed;
            case AttestationBudgetStatus.Done:
                return next == AttestationBudgetStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AttestationBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"attestation budget"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public AttestationBudget Copy()
    {
        return new AttestationBudget(Code, Label, LeaveDays, Channel, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
