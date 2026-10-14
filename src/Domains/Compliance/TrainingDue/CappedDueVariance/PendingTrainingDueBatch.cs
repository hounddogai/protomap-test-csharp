namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending training due batch is in its life cycle.</summary>
public enum PendingTrainingDueBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a pending training due batch and its current status.</summary>
public sealed class PendingTrainingDueBatch(string code, string label, int graceDays, bool pinned, Dictionary<string, string> attributes, string phone, PendingTrainingDueBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public bool Pinned { get; set; } = pinned;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Phone { get; set; } = phone;
    public PendingTrainingDueBatchStatus Status { get; set; } = status;

    /// <summary>Starts a pending training due batch in status open.</summary>
    public static PendingTrainingDueBatch NewPendingTrainingDueBatch(string code, string label)
    {
        return new PendingTrainingDueBatch(
            code,
            label,
            0,
            false,
            new Dictionary<string, string>(),
            "",
            PendingTrainingDueBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingTrainingDueBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingTrainingDueBatchStatus next)
    {
        switch (Status)
        {
            case PendingTrainingDueBatchStatus.Open:
                return next == PendingTrainingDueBatchStatus.InReview;
            case PendingTrainingDueBatchStatus.InReview:
                switch (next)
                {
                    case PendingTrainingDueBatchStatus.Reopened:
                        return true;
                }
                return false;
            case PendingTrainingDueBatchStatus.Resolved:
                switch (next)
                {
                    case PendingTrainingDueBatchStatus.Reopened:
                        return true;
                }
                return false;
            case PendingTrainingDueBatchStatus.Reopened:
                switch (next)
                {
                    case PendingTrainingDueBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingTrainingDueBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending training due batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return (double)GraceDays;
    }

    public PendingTrainingDueBatch Copy()
    {
        return new PendingTrainingDueBatch(Code, Label, GraceDays, Pinned, Attributes, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
