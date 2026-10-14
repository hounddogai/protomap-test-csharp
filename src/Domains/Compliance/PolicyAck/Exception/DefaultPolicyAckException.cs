namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default policy ack exception is in its life cycle.</summary>
public enum DefaultPolicyAckExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One default policy ack exception, with what the planners need to act on it.</summary>
public sealed class DefaultPolicyAckException(string code, string name, int graceDays, string notes, Dictionary<string, string> attributes, double hours, int createdAt, double weight, double cost, string ownerId, int windowDays, DefaultPolicyAckExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public string Notes { get; set; } = notes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Hours { get; set; } = hours;
    public int CreatedAt { get; set; } = createdAt;
    public double Weight { get; set; } = weight;
    public double Cost { get; set; } = cost;
    public string OwnerId { get; set; } = ownerId;
    public int WindowDays { get; set; } = windowDays;
    public DefaultPolicyAckExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a default policy ack exception in status requested.</summary>
    public static DefaultPolicyAckException NewDefaultPolicyAckException(string code, string name)
    {
        return new DefaultPolicyAckException(
            code,
            name,
            0,
            "",
            new Dictionary<string, string>(),
            0.0,
            0,
            0.0,
            0.0,
            "",
            0,
            DefaultPolicyAckExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPolicyAckExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPolicyAckExceptionStatus next)
    {
        switch (Status)
        {
            case DefaultPolicyAckExceptionStatus.Requested:
                return next == DefaultPolicyAckExceptionStatus.Revoked;
            case DefaultPolicyAckExceptionStatus.Granted:
                switch (next)
                {
                    case DefaultPolicyAckExceptionStatus.Revoked:
                    case DefaultPolicyAckExceptionStatus.Expired:
                        return true;
                }
                return false;
            case DefaultPolicyAckExceptionStatus.Revoked:
                switch (next)
                {
                    case DefaultPolicyAckExceptionStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPolicyAckExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default policy ack exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double NetVolume()
    {
        return Math.Round((double)CreatedAt - Cost, 2);
    }

    public DefaultPolicyAckException Copy()
    {
        return new DefaultPolicyAckException(
            Code,
            Name,
            GraceDays,
            Notes,
            Attributes,
            Hours,
            CreatedAt,
            Weight,
            Cost,
            OwnerId,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
