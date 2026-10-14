namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy offboarding request is in its life cycle.</summary>
public enum LegacyOffboardingRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a legacy offboarding request and its current status.</summary>
public sealed class LegacyOffboardingRequest(string code, string label, string timezone, int expiresAt, string jobCode, double threshold, LegacyOffboardingRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public int ExpiresAt { get; set; } = expiresAt;
    public string JobCode { get; set; } = jobCode;
    public double Threshold { get; set; } = threshold;
    public LegacyOffboardingRequestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy offboarding request in status new.</summary>
    public static LegacyOffboardingRequest NewLegacyOffboardingRequest(string code, string label)
    {
        return new LegacyOffboardingRequest(code, label, "", 0, "", 0.0, LegacyOffboardingRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyOffboardingRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyOffboardingRequestStatus next)
    {
        switch (Status)
        {
            case LegacyOffboardingRequestStatus.New:
                switch (next)
                {
                    case LegacyOffboardingRequestStatus.Failed:
                        return true;
                }
                return false;
            case LegacyOffboardingRequestStatus.Queued:
                return next == LegacyOffboardingRequestStatus.Done;
            case LegacyOffboardingRequestStatus.Running:
                return next == LegacyOffboardingRequestStatus.Done || next == LegacyOffboardingRequestStatus.Failed;
            case LegacyOffboardingRequestStatus.Done:
                return next == LegacyOffboardingRequestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyOffboardingRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy offboarding request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"legacy offboarding request"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)ExpiresAt + Threshold, 2);
    }

    public LegacyOffboardingRequest Copy()
    {
        return new LegacyOffboardingRequest(Code, Label, Timezone, ExpiresAt, JobCode, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
