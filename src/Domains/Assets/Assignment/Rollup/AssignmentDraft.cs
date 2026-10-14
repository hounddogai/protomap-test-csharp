namespace Acme.Workforce.Assets.Assignment.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment draft is in its life cycle.</summary>
public enum AssignmentDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A assignment draft. Its code is unique within a tenant.</summary>
public sealed class AssignmentDraft(string code, string label, int retries, double rating, int expiresAt, double weight, double fte, double floorValue, int windowDays, AssignmentDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public double Rating { get; set; } = rating;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Weight { get; set; } = weight;
    public double Fte { get; set; } = fte;
    public double FloorValue { get; set; } = floorValue;
    public int WindowDays { get; set; } = windowDays;
    public AssignmentDraftStatus Status { get; set; } = status;

    /// <summary>Starts a assignment draft in status requested.</summary>
    public static AssignmentDraft NewAssignmentDraft(string code, string label)
    {
        return new AssignmentDraft(code, label, 0, 0.0, 0, 0.0, 0.0, 0.0, 0, AssignmentDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentDraftStatus next)
    {
        switch (Status)
        {
            case AssignmentDraftStatus.Requested:
                switch (next)
                {
                    case AssignmentDraftStatus.Revoked:
                        return true;
                }
                return false;
            case AssignmentDraftStatus.Granted:
                return next == AssignmentDraftStatus.Expired;
            case AssignmentDraftStatus.Revoked:
                switch (next)
                {
                    case AssignmentDraftStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"assignment draft"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)WindowDays + Rating, 2);
    }

    public AssignmentDraft Copy()
    {
        return new AssignmentDraft(
            Code,
            Label,
            Retries,
            Rating,
            ExpiresAt,
            Weight,
            Fte,
            FloorValue,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
