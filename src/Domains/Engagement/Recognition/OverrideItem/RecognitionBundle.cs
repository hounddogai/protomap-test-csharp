namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition bundle is in its life cycle.</summary>
public enum RecognitionBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a recognition bundle and its current status.</summary>
public sealed class RecognitionBundle(string code, string name, string ownerId, string checksum, int endsAt, int startsAt, bool active, string country, RecognitionBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public string Checksum { get; set; } = checksum;
    public int EndsAt { get; set; } = endsAt;
    public int StartsAt { get; set; } = startsAt;
    public bool Active { get; set; } = active;
    public string Country { get; set; } = country;
    public RecognitionBundleStatus Status { get; set; } = status;

    /// <summary>Starts a recognition bundle in status open.</summary>
    public static RecognitionBundle NewRecognitionBundle(string code, string name)
    {
        return new RecognitionBundle(code, name, "", "", 0, 0, false, "", RecognitionBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionBundleStatus next)
    {
        switch (Status)
        {
            case RecognitionBundleStatus.Open:
                switch (next)
                {
                    case RecognitionBundleStatus.Reopened:
                    case RecognitionBundleStatus.Archived:
                        return true;
                }
                return false;
            case RecognitionBundleStatus.InReview:
                return next == RecognitionBundleStatus.Resolved;
            case RecognitionBundleStatus.Resolved:
                return next == RecognitionBundleStatus.Reopened || next == RecognitionBundleStatus.Archived;
            case RecognitionBundleStatus.Reopened:
                return next == RecognitionBundleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RecognitionBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)StartsAt + (double)EndsAt, 2);
    }

    public RecognitionBundle Copy()
    {
        return new RecognitionBundle(Code, Name, OwnerId, Checksum, EndsAt, StartsAt, Active, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
