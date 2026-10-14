namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict policy rollup is in its life cycle.</summary>
public enum StrictPolicyRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A strict policy rollup. Its code is unique within a tenant.</summary>
public sealed class StrictPolicyRollup(string code, string label, int attempts, List<string> tags, int capacity, string contactEmail, double rating, double rate, bool pinned, bool active, int expiresAt, StrictPolicyRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public List<string> Tags { get; set; } = tags;
    public int Capacity { get; set; } = capacity;
    public string ContactEmail { get; set; } = contactEmail;
    public double Rating { get; set; } = rating;
    public double Rate { get; set; } = rate;
    public bool Pinned { get; set; } = pinned;
    public bool Active { get; set; } = active;
    public int ExpiresAt { get; set; } = expiresAt;
    public StrictPolicyRollupStatus Status { get; set; } = status;

    /// <summary>Starts a strict policy rollup in status created.</summary>
    public static StrictPolicyRollup NewStrictPolicyRollup(string code, string label)
    {
        return new StrictPolicyRollup(
            code,
            label,
            0,
            new List<string>(),
            0,
            "",
            0.0,
            0.0,
            false,
            false,
            0,
            StrictPolicyRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPolicyRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPolicyRollupStatus next)
    {
        switch (Status)
        {
            case StrictPolicyRollupStatus.Created:
                switch (next)
                {
                    case StrictPolicyRollupStatus.Validated:
                    case StrictPolicyRollupStatus.Retired:
                        return true;
                }
                return false;
            case StrictPolicyRollupStatus.Validated:
                switch (next)
                {
                    case StrictPolicyRollupStatus.Published:
                    case StrictPolicyRollupStatus.Retired:
                        return true;
                }
                return false;
            case StrictPolicyRollupStatus.Published:
                switch (next)
                {
                    case StrictPolicyRollupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictPolicyRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict policy rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double ProjectedTotal()
    {
        return Math.Round(Rating - (double)Attempts, 2);
    }

    public StrictPolicyRollup Copy()
    {
        return new StrictPolicyRollup(
            Code,
            Label,
            Attempts,
            Tags,
            Capacity,
            ContactEmail,
            Rating,
            Rate,
            Pinned,
            Active,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
