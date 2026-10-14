namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition preference is in its life cycle.</summary>
public enum RecognitionPreferenceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a recognition preference and its current status.</summary>
public sealed class RecognitionPreference(string code, string label, int expiresAt, int endsAt, string unit, RecognitionPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public int EndsAt { get; set; } = endsAt;
    public string Unit { get; set; } = unit;
    public RecognitionPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a recognition preference in status proposed.</summary>
    public static RecognitionPreference NewRecognitionPreference(string code, string label)
    {
        return new RecognitionPreference(code, label, 0, 0, "", RecognitionPreferenceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionPreferenceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionPreferenceStatus next)
    {
        switch (Status)
        {
            case RecognitionPreferenceStatus.Proposed:
                return next == RecognitionPreferenceStatus.Withdrawn;
            case RecognitionPreferenceStatus.Approved:
                switch (next)
                {
                    case RecognitionPreferenceStatus.Rejected:
                    case RecognitionPreferenceStatus.Withdrawn:
                        return true;
                }
                return false;
            case RecognitionPreferenceStatus.Rejected:
                return next == RecognitionPreferenceStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RecognitionPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"recognition preference"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)EndsAt * (double)ExpiresAt, 2);
    }

    public RecognitionPreference Copy()
    {
        return new RecognitionPreference(Code, Label, ExpiresAt, EndsAt, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
