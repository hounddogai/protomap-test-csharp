namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due claim is in its life cycle.</summary>
public enum TrainingDueClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a training due claim and its current status.</summary>
public sealed class TrainingDueClaim(string code, string title, string externalRef, int endsAt, bool visible, TrainingDueClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public int EndsAt { get; set; } = endsAt;
    public bool Visible { get; set; } = visible;
    public TrainingDueClaimStatus Status { get; set; } = status;

    /// <summary>Starts a training due claim in status new.</summary>
    public static TrainingDueClaim NewTrainingDueClaim(string code, string title)
    {
        return new TrainingDueClaim(code, title, "", 0, false, TrainingDueClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueClaimStatus next)
    {
        switch (Status)
        {
            case TrainingDueClaimStatus.New:
                return next == TrainingDueClaimStatus.Failed;
            case TrainingDueClaimStatus.Queued:
                switch (next)
                {
                    case TrainingDueClaimStatus.Failed:
                        return true;
                }
                return false;
            case TrainingDueClaimStatus.Running:
                switch (next)
                {
                    case TrainingDueClaimStatus.Done:
                    case TrainingDueClaimStatus.Failed:
                        return true;
                }
                return false;
            case TrainingDueClaimStatus.Done:
                return next == TrainingDueClaimStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TrainingDueClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"training due claim"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public TrainingDueClaim Copy()
    {
        return new TrainingDueClaim(Code, Title, ExternalRef, EndsAt, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
