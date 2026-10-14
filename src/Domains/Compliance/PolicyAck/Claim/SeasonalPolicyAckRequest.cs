namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal policy ack request is in its life cycle.</summary>
public enum SeasonalPolicyAckRequestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One seasonal policy ack request, with what the planners need to act on it.</summary>
public sealed class SeasonalPolicyAckRequest(string code, string label, bool approved, Dictionary<string, string> attributes, double cost, List<string> tags, SeasonalPolicyAckRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Cost { get; set; } = cost;
    public List<string> Tags { get; set; } = tags;
    public SeasonalPolicyAckRequestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal policy ack request in status requested.</summary>
    public static SeasonalPolicyAckRequest NewSeasonalPolicyAckRequest(string code, string label)
    {
        return new SeasonalPolicyAckRequest(
            code,
            label,
            false,
            new Dictionary<string, string>(),
            0.0,
            new List<string>(),
            SeasonalPolicyAckRequestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPolicyAckRequestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPolicyAckRequestStatus next)
    {
        switch (Status)
        {
            case SeasonalPolicyAckRequestStatus.Requested:
                return next == SeasonalPolicyAckRequestStatus.Expired;
            case SeasonalPolicyAckRequestStatus.Granted:
                return next == SeasonalPolicyAckRequestStatus.Expired;
            case SeasonalPolicyAckRequestStatus.Revoked:
                switch (next)
                {
                    case SeasonalPolicyAckRequestStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPolicyAckRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal policy ack request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
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

    public double MeasuredValue()
    {
        return Cost;
    }

    public SeasonalPolicyAckRequest Copy()
    {
        return new SeasonalPolicyAckRequest(Code, Label, Approved, Attributes, Cost, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
