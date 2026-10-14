namespace Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey snapshot is in its life cycle.</summary>
public enum PulseSurveySnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pulse survey snapshot and its current status.</summary>
public sealed class PulseSurveySnapshot(string code, string title, int retries, string location, string externalRef, string currency, PulseSurveySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string Location { get; set; } = location;
    public string ExternalRef { get; set; } = externalRef;
    public string Currency { get; set; } = currency;
    public PulseSurveySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey snapshot in status idle.</summary>
    public static PulseSurveySnapshot NewPulseSurveySnapshot(string code, string title)
    {
        return new PulseSurveySnapshot(code, title, 0, "", "", "", PulseSurveySnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveySnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveySnapshotStatus next)
    {
        switch (Status)
        {
            case PulseSurveySnapshotStatus.Idle:
                return next == PulseSurveySnapshotStatus.Warming || next == PulseSurveySnapshotStatus.Ready;
            case PulseSurveySnapshotStatus.Warming:
                switch (next)
                {
                    case PulseSurveySnapshotStatus.Ready:
                    case PulseSurveySnapshotStatus.Draining:
                        return true;
                }
                return false;
            case PulseSurveySnapshotStatus.Ready:
                return next == PulseSurveySnapshotStatus.Draining || next == PulseSurveySnapshotStatus.Stopped;
            case PulseSurveySnapshotStatus.Draining:
                switch (next)
                {
                    case PulseSurveySnapshotStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pulse survey snapshot"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public PulseSurveySnapshot Copy()
    {
        return new PulseSurveySnapshot(Code, Title, Retries, Location, ExternalRef, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
