namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation summary is in its life cycle.</summary>
public enum AttestationSummaryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A attestation summary as the attestation team keeps it.</summary>
public sealed class AttestationSummary(string code, string label, string description, double budget, double rate, string employeeId, string channel, int priority, bool archived, string badge, List<string> tags, AttestationSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public double Budget { get; set; } = budget;
    public double Rate { get; set; } = rate;
    public string EmployeeId { get; set; } = employeeId;
    public string Channel { get; set; } = channel;
    public int Priority { get; set; } = priority;
    public bool Archived { get; set; } = archived;
    public string Badge { get; set; } = badge;
    public List<string> Tags { get; set; } = tags;
    public AttestationSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a attestation summary in status created.</summary>
    public static AttestationSummary NewAttestationSummary(string code, string label)
    {
        return new AttestationSummary(
            code,
            label,
            "",
            0.0,
            0.0,
            "",
            "",
            0,
            false,
            "",
            new List<string>(),
            AttestationSummaryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationSummaryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationSummaryStatus next)
    {
        switch (Status)
        {
            case AttestationSummaryStatus.Created:
                switch (next)
                {
                    case AttestationSummaryStatus.Published:
                        return true;
                }
                return false;
            case AttestationSummaryStatus.Validated:
                return next == AttestationSummaryStatus.Published;
            case AttestationSummaryStatus.Published:
                switch (next)
                {
                    case AttestationSummaryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double ProjectedExposure()
    {
        return Math.Round((double)Priority * Rate, 2);
    }

    public AttestationSummary Copy()
    {
        return new AttestationSummary(
            Code,
            Label,
            Description,
            Budget,
            Rate,
            EmployeeId,
            Channel,
            Priority,
            Archived,
            Badge,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
