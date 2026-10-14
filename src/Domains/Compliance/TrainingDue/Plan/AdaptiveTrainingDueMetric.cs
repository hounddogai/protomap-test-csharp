namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive training due metric is in its life cycle.</summary>
public enum AdaptiveTrainingDueMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a adaptive training due metric and its current status.</summary>
public sealed class AdaptiveTrainingDueMetric(string code, string label, List<string> tags, int sequence, string checksum, double weight, AdaptiveTrainingDueMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public int Sequence { get; set; } = sequence;
    public string Checksum { get; set; } = checksum;
    public double Weight { get; set; } = weight;
    public AdaptiveTrainingDueMetricStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive training due metric in status created.</summary>
    public static AdaptiveTrainingDueMetric NewAdaptiveTrainingDueMetric(string code, string label)
    {
        return new AdaptiveTrainingDueMetric(
            code,
            label,
            new List<string>(),
            0,
            "",
            0.0,
            AdaptiveTrainingDueMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTrainingDueMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTrainingDueMetricStatus next)
    {
        switch (Status)
        {
            case AdaptiveTrainingDueMetricStatus.Created:
                switch (next)
                {
                    case AdaptiveTrainingDueMetricStatus.Validated:
                        return true;
                }
                return false;
            case AdaptiveTrainingDueMetricStatus.Validated:
                return next == AdaptiveTrainingDueMetricStatus.Published;
            case AdaptiveTrainingDueMetricStatus.Published:
                return next == AdaptiveTrainingDueMetricStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveTrainingDueMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive training due metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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

    public double NetVolume()
    {
        return Math.Round(Weight * (double)Sequence, 2);
    }

    public AdaptiveTrainingDueMetric Copy()
    {
        return new AdaptiveTrainingDueMetric(Code, Label, Tags, Sequence, Checksum, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
