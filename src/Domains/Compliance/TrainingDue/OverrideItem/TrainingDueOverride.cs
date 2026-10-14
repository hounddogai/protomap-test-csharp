namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due override is in its life cycle.</summary>
public enum TrainingDueOverrideStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A training due override as the training due team keeps it.</summary>
public sealed class TrainingDueOverride(string code, string label, bool approved, string managerId, bool archived, List<string> tags, double discount, TrainingDueOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public string ManagerId { get; set; } = managerId;
    public bool Archived { get; set; } = archived;
    public List<string> Tags { get; set; } = tags;
    public double Discount { get; set; } = discount;
    public TrainingDueOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a training due override in status open.</summary>
    public static TrainingDueOverride NewTrainingDueOverride(string code, string label)
    {
        return new TrainingDueOverride(
            code,
            label,
            false,
            "",
            false,
            new List<string>(),
            0.0,
            TrainingDueOverrideStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueOverrideStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueOverrideStatus next)
    {
        switch (Status)
        {
            case TrainingDueOverrideStatus.Open:
                return next == TrainingDueOverrideStatus.Archived;
            case TrainingDueOverrideStatus.InReview:
                switch (next)
                {
                    case TrainingDueOverrideStatus.Archived:
                        return true;
                }
                return false;
            case TrainingDueOverrideStatus.Resolved:
                return next == TrainingDueOverrideStatus.Reopened || next == TrainingDueOverrideStatus.Archived;
            case TrainingDueOverrideStatus.Reopened:
                return next == TrainingDueOverrideStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TrainingDueOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"training due override"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public TrainingDueOverride Copy()
    {
        return new TrainingDueOverride(Code, Label, Approved, ManagerId, Archived, Tags, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
