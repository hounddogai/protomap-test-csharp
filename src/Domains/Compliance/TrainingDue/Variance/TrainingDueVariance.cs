namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due variance is in its life cycle.</summary>
public enum TrainingDueVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A training due variance. Its code is unique within a tenant.</summary>
public sealed class TrainingDueVariance(string code, string name, int version, double threshold, int endsAt, int capacity, string department, string grade, double floorValue, string skill, Dictionary<string, string> attributes, TrainingDueVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public double Threshold { get; set; } = threshold;
    public int EndsAt { get; set; } = endsAt;
    public int Capacity { get; set; } = capacity;
    public string Department { get; set; } = department;
    public string Grade { get; set; } = grade;
    public double FloorValue { get; set; } = floorValue;
    public string Skill { get; set; } = skill;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public TrainingDueVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a training due variance in status new.</summary>
    public static TrainingDueVariance NewTrainingDueVariance(string code, string name)
    {
        return new TrainingDueVariance(
            code,
            name,
            0,
            0.0,
            0,
            0,
            "",
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            TrainingDueVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueVarianceStatus next)
    {
        switch (Status)
        {
            case TrainingDueVarianceStatus.New:
                switch (next)
                {
                    case TrainingDueVarianceStatus.Done:
                        return true;
                }
                return false;
            case TrainingDueVarianceStatus.Queued:
                return next == TrainingDueVarianceStatus.Running;
            case TrainingDueVarianceStatus.Running:
                switch (next)
                {
                    case TrainingDueVarianceStatus.Done:
                    case TrainingDueVarianceStatus.Failed:
                        return true;
                }
                return false;
            case TrainingDueVarianceStatus.Done:
                switch (next)
                {
                    case TrainingDueVarianceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double AdjustedLoad()
    {
        return Math.Round((double)Capacity + Threshold, 2);
    }

    public TrainingDueVariance Copy()
    {
        return new TrainingDueVariance(
            Code,
            Name,
            Version,
            Threshold,
            EndsAt,
            Capacity,
            Department,
            Grade,
            FloorValue,
            Skill,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
