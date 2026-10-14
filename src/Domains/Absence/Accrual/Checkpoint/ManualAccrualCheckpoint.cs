namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual accrual checkpoint is in its life cycle.</summary>
public enum ManualAccrualCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A manual accrual checkpoint as the accrual team keeps it.</summary>
public sealed class ManualAccrualCheckpoint(string code, string title, int sequence, List<string> tags, string summary, string label, double weight, string category, double rating, ManualAccrualCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public List<string> Tags { get; set; } = tags;
    public string Summary { get; set; } = summary;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public string Category { get; set; } = category;
    public double Rating { get; set; } = rating;
    public ManualAccrualCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a manual accrual checkpoint in status requested.</summary>
    public static ManualAccrualCheckpoint NewManualAccrualCheckpoint(string code, string title)
    {
        return new ManualAccrualCheckpoint(
            code,
            title,
            0,
            new List<string>(),
            "",
            "",
            0.0,
            "",
            0.0,
            ManualAccrualCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAccrualCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAccrualCheckpointStatus next)
    {
        switch (Status)
        {
            case ManualAccrualCheckpointStatus.Requested:
                return next == ManualAccrualCheckpointStatus.Granted;
            case ManualAccrualCheckpointStatus.Granted:
                return next == ManualAccrualCheckpointStatus.Revoked || next == ManualAccrualCheckpointStatus.Expired;
            case ManualAccrualCheckpointStatus.Revoked:
                return next == ManualAccrualCheckpointStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ManualAccrualCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual accrual checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
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

    public double EffectiveExposure()
    {
        return Math.Round((double)Sequence * Weight, 2);
    }

    public ManualAccrualCheckpoint Copy()
    {
        return new ManualAccrualCheckpoint(
            Code,
            Title,
            Sequence,
            Tags,
            Summary,
            Label,
            Weight,
            Category,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
