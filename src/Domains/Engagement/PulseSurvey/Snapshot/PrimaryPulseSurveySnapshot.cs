namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary pulse survey snapshot is in its life cycle.</summary>
public enum PrimaryPulseSurveySnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a primary pulse survey snapshot and its current status.</summary>
public sealed class PrimaryPulseSurveySnapshot(string code, string name, double weight, string summary, string phone, double rating, string ownerId, string notes, PrimaryPulseSurveySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Summary { get; set; } = summary;
    public string Phone { get; set; } = phone;
    public double Rating { get; set; } = rating;
    public string OwnerId { get; set; } = ownerId;
    public string Notes { get; set; } = notes;
    public PrimaryPulseSurveySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a primary pulse survey snapshot in status draft.</summary>
    public static PrimaryPulseSurveySnapshot NewPrimaryPulseSurveySnapshot(string code, string name)
    {
        return new PrimaryPulseSurveySnapshot(
            code,
            name,
            0.0,
            "",
            "",
            0.0,
            "",
            "",
            PrimaryPulseSurveySnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPulseSurveySnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPulseSurveySnapshotStatus next)
    {
        switch (Status)
        {
            case PrimaryPulseSurveySnapshotStatus.Draft:
                switch (next)
                {
                    case PrimaryPulseSurveySnapshotStatus.Pending:
                    case PrimaryPulseSurveySnapshotStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryPulseSurveySnapshotStatus.Pending:
                return next == PrimaryPulseSurveySnapshotStatus.Active || next == PrimaryPulseSurveySnapshotStatus.Suspended;
            case PrimaryPulseSurveySnapshotStatus.Active:
                return next == PrimaryPulseSurveySnapshotStatus.Suspended;
            case PrimaryPulseSurveySnapshotStatus.Suspended:
                return next == PrimaryPulseSurveySnapshotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimaryPulseSurveySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary pulse survey snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetVolume()
    {
        return Math.Round(Weight - Rating, 2);
    }

    public PrimaryPulseSurveySnapshot Copy()
    {
        return new PrimaryPulseSurveySnapshot(Code, Name, Weight, Summary, Phone, Rating, OwnerId, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
