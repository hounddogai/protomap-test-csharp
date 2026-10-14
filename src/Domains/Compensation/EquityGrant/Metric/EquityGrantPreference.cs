namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant preference is in its life cycle.</summary>
public enum EquityGrantPreferenceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A equity grant preference. Its code is unique within a tenant.</summary>
public sealed class EquityGrantPreference(string code, string label, int revision, int priority, int endsAt, bool pinned, int version, string timezone, string location, string jobCode, double margin, EquityGrantPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int Priority { get; set; } = priority;
    public int EndsAt { get; set; } = endsAt;
    public bool Pinned { get; set; } = pinned;
    public int Version { get; set; } = version;
    public string Timezone { get; set; } = timezone;
    public string Location { get; set; } = location;
    public string JobCode { get; set; } = jobCode;
    public double Margin { get; set; } = margin;
    public EquityGrantPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant preference in status draft.</summary>
    public static EquityGrantPreference NewEquityGrantPreference(string code, string label)
    {
        return new EquityGrantPreference(
            code,
            label,
            0,
            0,
            0,
            false,
            0,
            "",
            "",
            "",
            0.0,
            EquityGrantPreferenceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantPreferenceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantPreferenceStatus next)
    {
        switch (Status)
        {
            case EquityGrantPreferenceStatus.Draft:
                switch (next)
                {
                    case EquityGrantPreferenceStatus.Closed:
                        return true;
                }
                return false;
            case EquityGrantPreferenceStatus.Pending:
                switch (next)
                {
                    case EquityGrantPreferenceStatus.Active:
                    case EquityGrantPreferenceStatus.Closed:
                        return true;
                }
                return false;
            case EquityGrantPreferenceStatus.Active:
                switch (next)
                {
                    case EquityGrantPreferenceStatus.Suspended:
                        return true;
                }
                return false;
            case EquityGrantPreferenceStatus.Suspended:
                switch (next)
                {
                    case EquityGrantPreferenceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Priority + (double)Revision, 2);
    }

    public EquityGrantPreference Copy()
    {
        return new EquityGrantPreference(
            Code,
            Label,
            Revision,
            Priority,
            EndsAt,
            Pinned,
            Version,
            Timezone,
            Location,
            JobCode,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
