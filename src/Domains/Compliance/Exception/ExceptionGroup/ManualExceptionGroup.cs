namespace Acme.Workforce.Compliance.Exception.ExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual exception group is in its life cycle.</summary>
public enum ManualExceptionGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One manual exception group, with what the planners need to act on it.</summary>
public sealed class ManualExceptionGroup(string code, string name, Dictionary<string, string> attributes, int attempts, int updatedAt, double ceiling, string grade, bool pinned, string managerId, string currency, string contactEmail, ManualExceptionGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Attempts { get; set; } = attempts;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Ceiling { get; set; } = ceiling;
    public string Grade { get; set; } = grade;
    public bool Pinned { get; set; } = pinned;
    public string ManagerId { get; set; } = managerId;
    public string Currency { get; set; } = currency;
    public string ContactEmail { get; set; } = contactEmail;
    public ManualExceptionGroupStatus Status { get; set; } = status;

    /// <summary>Starts a manual exception group in status idle.</summary>
    public static ManualExceptionGroup NewManualExceptionGroup(string code, string name)
    {
        return new ManualExceptionGroup(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            0,
            0.0,
            "",
            false,
            "",
            "",
            "",
            ManualExceptionGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualExceptionGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualExceptionGroupStatus next)
    {
        switch (Status)
        {
            case ManualExceptionGroupStatus.Idle:
                return next == ManualExceptionGroupStatus.Draining || next == ManualExceptionGroupStatus.Stopped;
            case ManualExceptionGroupStatus.Warming:
                switch (next)
                {
                    case ManualExceptionGroupStatus.Ready:
                    case ManualExceptionGroupStatus.Stopped:
                        return true;
                }
                return false;
            case ManualExceptionGroupStatus.Ready:
                return next == ManualExceptionGroupStatus.Draining || next == ManualExceptionGroupStatus.Stopped;
            case ManualExceptionGroupStatus.Draining:
                return next == ManualExceptionGroupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualExceptionGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual exception group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double ProjectedLoad()
    {
        return Math.Round(Ceiling + (double)UpdatedAt, 2);
    }

    public ManualExceptionGroup Copy()
    {
        return new ManualExceptionGroup(
            Code,
            Name,
            Attributes,
            Attempts,
            UpdatedAt,
            Ceiling,
            Grade,
            Pinned,
            ManagerId,
            Currency,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
