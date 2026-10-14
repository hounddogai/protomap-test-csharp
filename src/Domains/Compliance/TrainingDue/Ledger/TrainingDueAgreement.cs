namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due agreement is in its life cycle.</summary>
public enum TrainingDueAgreementStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A training due agreement. Its code is unique within a tenant.</summary>
public sealed class TrainingDueAgreement(string code, string label, string contactEmail, double score, string source, TrainingDueAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public double Score { get; set; } = score;
    public string Source { get; set; } = source;
    public TrainingDueAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a training due agreement in status open.</summary>
    public static TrainingDueAgreement NewTrainingDueAgreement(string code, string label)
    {
        return new TrainingDueAgreement(code, label, "", 0.0, "", TrainingDueAgreementStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueAgreementStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueAgreementStatus next)
    {
        switch (Status)
        {
            case TrainingDueAgreementStatus.Open:
                switch (next)
                {
                    case TrainingDueAgreementStatus.Reopened:
                        return true;
                }
                return false;
            case TrainingDueAgreementStatus.InReview:
                return next == TrainingDueAgreementStatus.Reopened;
            case TrainingDueAgreementStatus.Resolved:
                switch (next)
                {
                    case TrainingDueAgreementStatus.Reopened:
                    case TrainingDueAgreementStatus.Archived:
                        return true;
                }
                return false;
            case TrainingDueAgreementStatus.Reopened:
                return next == TrainingDueAgreementStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TrainingDueAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public TrainingDueAgreement Copy()
    {
        return new TrainingDueAgreement(Code, Label, ContactEmail, Score, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
