namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due threshold is in its life cycle.</summary>
public enum TrainingDueThresholdStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A training due threshold as the training due team keeps it.</summary>
public sealed class TrainingDueThreshold(string code, string label, int attempts, double cost, string country, double ceiling, int retries, double fte, List<string> tags, TrainingDueThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public double Cost { get; set; } = cost;
    public string Country { get; set; } = country;
    public double Ceiling { get; set; } = ceiling;
    public int Retries { get; set; } = retries;
    public double Fte { get; set; } = fte;
    public List<string> Tags { get; set; } = tags;
    public TrainingDueThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a training due threshold in status created.</summary>
    public static TrainingDueThreshold NewTrainingDueThreshold(string code, string label)
    {
        return new TrainingDueThreshold(
            code,
            label,
            0,
            0.0,
            "",
            0.0,
            0,
            0.0,
            new List<string>(),
            TrainingDueThresholdStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueThresholdStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueThresholdStatus next)
    {
        switch (Status)
        {
            case TrainingDueThresholdStatus.Created:
                return next == TrainingDueThresholdStatus.Published || next == TrainingDueThresholdStatus.Retired;
            case TrainingDueThresholdStatus.Validated:
                return next == TrainingDueThresholdStatus.Published || next == TrainingDueThresholdStatus.Retired;
            case TrainingDueThresholdStatus.Published:
                return next == TrainingDueThresholdStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TrainingDueThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"training due threshold"} {Code}";
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

    public double AdjustedValue()
    {
        return Math.Round((double)Retries * Cost, 2);
    }

    public TrainingDueThreshold Copy()
    {
        return new TrainingDueThreshold(Code, Label, Attempts, Cost, Country, Ceiling, Retries, Fte, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
