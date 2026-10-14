namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due digest is in its life cycle.</summary>
public enum TrainingDueDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A training due digest as the training due team keeps it.</summary>
public sealed class TrainingDueDigest(string code, string label, int endsAt, int startsAt, string managerId, List<string> tags, string summary, string country, double rating, int updatedAt, TrainingDueDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public int StartsAt { get; set; } = startsAt;
    public string ManagerId { get; set; } = managerId;
    public List<string> Tags { get; set; } = tags;
    public string Summary { get; set; } = summary;
    public string Country { get; set; } = country;
    public double Rating { get; set; } = rating;
    public int UpdatedAt { get; set; } = updatedAt;
    public TrainingDueDigestStatus Status { get; set; } = status;

    /// <summary>Starts a training due digest in status requested.</summary>
    public static TrainingDueDigest NewTrainingDueDigest(string code, string label)
    {
        return new TrainingDueDigest(
            code,
            label,
            0,
            0,
            "",
            new List<string>(),
            "",
            "",
            0.0,
            0,
            TrainingDueDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueDigestStatus next)
    {
        switch (Status)
        {
            case TrainingDueDigestStatus.Requested:
                switch (next)
                {
                    case TrainingDueDigestStatus.Granted:
                    case TrainingDueDigestStatus.Expired:
                        return true;
                }
                return false;
            case TrainingDueDigestStatus.Granted:
                return next == TrainingDueDigestStatus.Revoked || next == TrainingDueDigestStatus.Expired;
            case TrainingDueDigestStatus.Revoked:
                switch (next)
                {
                    case TrainingDueDigestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double NetLoad()
    {
        return Math.Round((double)StartsAt * (double)EndsAt, 2);
    }

    public TrainingDueDigest Copy()
    {
        return new TrainingDueDigest(
            Code,
            Label,
            EndsAt,
            StartsAt,
            ManagerId,
            Tags,
            Summary,
            Country,
            Rating,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
