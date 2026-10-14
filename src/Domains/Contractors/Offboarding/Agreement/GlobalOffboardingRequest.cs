namespace Acme.Workforce.Contractors.Offboarding.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global offboarding request is in its life cycle.</summary>
public enum GlobalOffboardingRequestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a global offboarding request and its current status.</summary>
public sealed class GlobalOffboardingRequest(string code, string label, string category, string contactEmail, string team, double score, GlobalOffboardingRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public string ContactEmail { get; set; } = contactEmail;
    public string Team { get; set; } = team;
    public double Score { get; set; } = score;
    public GlobalOffboardingRequestStatus Status { get; set; } = status;

    /// <summary>Starts a global offboarding request in status requested.</summary>
    public static GlobalOffboardingRequest NewGlobalOffboardingRequest(string code, string label)
    {
        return new GlobalOffboardingRequest(code, label, "", "", "", 0.0, GlobalOffboardingRequestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalOffboardingRequestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalOffboardingRequestStatus next)
    {
        switch (Status)
        {
            case GlobalOffboardingRequestStatus.Requested:
                switch (next)
                {
                    case GlobalOffboardingRequestStatus.Granted:
                        return true;
                }
                return false;
            case GlobalOffboardingRequestStatus.Granted:
                return next == GlobalOffboardingRequestStatus.Expired;
            case GlobalOffboardingRequestStatus.Revoked:
                return next == GlobalOffboardingRequestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(GlobalOffboardingRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global offboarding request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"global offboarding request"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public GlobalOffboardingRequest Copy()
    {
        return new GlobalOffboardingRequest(Code, Label, Category, ContactEmail, Team, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
