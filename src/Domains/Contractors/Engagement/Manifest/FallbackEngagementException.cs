namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback engagement exception is in its life cycle.</summary>
public enum FallbackEngagementExceptionStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One fallback engagement exception, with what the planners need to act on it.</summary>
public sealed class FallbackEngagementException(string code, string label, string segment, string timezone, string externalRef, int leaveDays, Dictionary<string, string> attributes, double rate, string costCenter, FallbackEngagementExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string Timezone { get; set; } = timezone;
    public string ExternalRef { get; set; } = externalRef;
    public int LeaveDays { get; set; } = leaveDays;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public string CostCenter { get; set; } = costCenter;
    public FallbackEngagementExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a fallback engagement exception in status created.</summary>
    public static FallbackEngagementException NewFallbackEngagementException(string code, string label)
    {
        return new FallbackEngagementException(
            code,
            label,
            "",
            "",
            "",
            0,
            new Dictionary<string, string>(),
            0.0,
            "",
            FallbackEngagementExceptionStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackEngagementExceptionStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackEngagementExceptionStatus next)
    {
        switch (Status)
        {
            case FallbackEngagementExceptionStatus.Created:
                switch (next)
                {
                    case FallbackEngagementExceptionStatus.Validated:
                    case FallbackEngagementExceptionStatus.Published:
                        return true;
                }
                return false;
            case FallbackEngagementExceptionStatus.Validated:
                return next == FallbackEngagementExceptionStatus.Published || next == FallbackEngagementExceptionStatus.Retired;
            case FallbackEngagementExceptionStatus.Published:
                switch (next)
                {
                    case FallbackEngagementExceptionStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackEngagementExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback engagement exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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
        return Math.Round((double)LeaveDays * Rate, 2);
    }

    public FallbackEngagementException Copy()
    {
        return new FallbackEngagementException(
            Code,
            Label,
            Segment,
            Timezone,
            ExternalRef,
            LeaveDays,
            Attributes,
            Rate,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
