namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending exception manifest is in its life cycle.</summary>
public enum PendingExceptionManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One pending exception manifest, with what the planners need to act on it.</summary>
public sealed class PendingExceptionManifest(string code, string label, Dictionary<string, string> attributes, string managerId, int retries, string phone, bool approved, string category, double hours, double floorValue, PendingExceptionManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ManagerId { get; set; } = managerId;
    public int Retries { get; set; } = retries;
    public string Phone { get; set; } = phone;
    public bool Approved { get; set; } = approved;
    public string Category { get; set; } = category;
    public double Hours { get; set; } = hours;
    public double FloorValue { get; set; } = floorValue;
    public PendingExceptionManifestStatus Status { get; set; } = status;

    /// <summary>Starts a pending exception manifest in status open.</summary>
    public static PendingExceptionManifest NewPendingExceptionManifest(string code, string label)
    {
        return new PendingExceptionManifest(
            code,
            label,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            false,
            "",
            0.0,
            0.0,
            PendingExceptionManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingExceptionManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingExceptionManifestStatus next)
    {
        switch (Status)
        {
            case PendingExceptionManifestStatus.Open:
                switch (next)
                {
                    case PendingExceptionManifestStatus.Reopened:
                        return true;
                }
                return false;
            case PendingExceptionManifestStatus.InReview:
                switch (next)
                {
                    case PendingExceptionManifestStatus.Resolved:
                    case PendingExceptionManifestStatus.Archived:
                        return true;
                }
                return false;
            case PendingExceptionManifestStatus.Resolved:
                switch (next)
                {
                    case PendingExceptionManifestStatus.Reopened:
                    case PendingExceptionManifestStatus.Archived:
                        return true;
                }
                return false;
            case PendingExceptionManifestStatus.Reopened:
                return next == PendingExceptionManifestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PendingExceptionManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending exception manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double NetValue()
    {
        return Math.Round((double)Retries * Hours, 2);
    }

    public PendingExceptionManifest Copy()
    {
        return new PendingExceptionManifest(
            Code,
            Label,
            Attributes,
            ManagerId,
            Retries,
            Phone,
            Approved,
            Category,
            Hours,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
