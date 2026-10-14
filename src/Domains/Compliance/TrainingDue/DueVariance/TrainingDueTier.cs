namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due tier is in its life cycle.</summary>
public enum TrainingDueTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A training due tier as the training due team keeps it.</summary>
public sealed class TrainingDueTier(string code, string label, int updatedAt, string department, string category, string channel, bool pinned, TrainingDueTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Department { get; set; } = department;
    public string Category { get; set; } = category;
    public string Channel { get; set; } = channel;
    public bool Pinned { get; set; } = pinned;
    public TrainingDueTierStatus Status { get; set; } = status;

    /// <summary>Starts a training due tier in status created.</summary>
    public static TrainingDueTier NewTrainingDueTier(string code, string label)
    {
        return new TrainingDueTier(code, label, 0, "", "", "", false, TrainingDueTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueTierStatus next)
    {
        switch (Status)
        {
            case TrainingDueTierStatus.Created:
                return next == TrainingDueTierStatus.Published || next == TrainingDueTierStatus.Retired;
            case TrainingDueTierStatus.Validated:
                switch (next)
                {
                    case TrainingDueTierStatus.Published:
                        return true;
                }
                return false;
            case TrainingDueTierStatus.Published:
                return next == TrainingDueTierStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TrainingDueTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"training due tier"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public TrainingDueTier Copy()
    {
        return new TrainingDueTier(Code, Label, UpdatedAt, Department, Category, Channel, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
